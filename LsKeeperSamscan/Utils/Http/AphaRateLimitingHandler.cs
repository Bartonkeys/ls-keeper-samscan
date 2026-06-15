using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using LsKeeperSamscan.Config;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Utils.Http;

/// <summary>
/// Throttles outbound APHA HTTP requests to <see cref="AphaConfig.RateLimitPerSecond"/> calls per second
/// using a token-bucket algorithm. Placed outside the Polly resilience pipeline so retries (which are
/// already delayed by exponential back-off) do not consume an extra token.
/// </summary>
public sealed class AphaRateLimitingHandler : DelegatingHandler, IDisposable
{
    private readonly TokenBucketRateLimiter _limiter;
    private readonly ILogger<AphaRateLimitingHandler> _logger;

    public AphaRateLimitingHandler(
        IOptions<AphaConfig> options,
        ILogger<AphaRateLimitingHandler> logger)
    {
        var rps = options.Value.RateLimitPerSecond;
        _logger = logger;

        _limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            // Replenish `rps` tokens per second
            TokenLimit = rps,
            TokensPerPeriod = rps,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            // Allow a short burst before queueing (2× bucket)
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = rps * 2,
            AutoReplenishment = true
        });
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var lease = await _limiter.AcquireAsync(permitCount: 1, cancellationToken);

        if (!lease.IsAcquired)
        {
            _logger.LogWarning("APHA rate limit queue exhausted for {RequestUri}", request.RequestUri);
            throw new InvalidOperationException($"APHA rate limit queue exhausted for {request.RequestUri}");
        }

        return await base.SendAsync(request, cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    protected override void Dispose(bool disposing)
    {
        if (disposing) _limiter.Dispose();
        base.Dispose(disposing);
    }
}
