namespace LsKeeperSamscan.Clients.Apha;

public interface IAphaTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
