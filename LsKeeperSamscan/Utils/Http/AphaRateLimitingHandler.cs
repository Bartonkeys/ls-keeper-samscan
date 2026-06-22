namespace LsKeeperSamscan.Utils.Http;

/// <summary>
/// Throttles outbound APHA HTTP requests using the shared <see cref="AphaRateLimiter"/>.
/// Acquires a token from the rate limiter before each request, blocking until
/// one is available. The rate is controlled by <see cref="Config.AphaConfig.RateLimitPerSecond"/>.
/// </summary>
public sealed class AphaRateLimitingHandler(AphaRateLimiter rateLimiter, ILogger<AphaRateLimitingHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var lease = await rateLimiter.Limiter.AcquireAsync(permitCount: 1, cancellationToken);

        if (!lease.IsAcquired)
        {
            logger.LogWarning("APHA rate limit queue exhausted for {RequestUri}", request.RequestUri);
            throw new InvalidOperationException($"APHA rate limit queue exhausted for {request.RequestUri}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
