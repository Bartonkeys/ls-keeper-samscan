using System.Threading.RateLimiting;
using LsKeeperSamscan.Config;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Utils.Http;

/// <summary>
/// Singleton that owns the shared <see cref="TokenBucketRateLimiter"/> used by
/// <see cref="AphaRateLimitingHandler"/> to throttle outbound APHA requests.
/// Separating state from the handler allows the handler to be transient,
/// which is safe for <see cref="IHttpClientFactory"/> pipeline rotation.
/// </summary>
public sealed class AphaRateLimiter : IDisposable
{
    public TokenBucketRateLimiter Limiter { get; }

    public AphaRateLimiter(IOptions<AphaConfig> options)
    {
        var rps = options.Value.RateLimitPerSecond;
        Limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = rps,
            TokensPerPeriod = rps,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = rps * 2,
            AutoReplenishment = true
        });
    }

    public void Dispose() => Limiter.Dispose();
}
