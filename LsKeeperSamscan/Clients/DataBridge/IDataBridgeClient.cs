namespace LsKeeperSamscan.Clients.DataBridge;

public interface IDataBridgeClient
{
    Task<int> GetSamHoldingsCountAsync(CancellationToken cancellationToken = default);
    Task<List<string>> ListSamHoldingsAsync(int skip, int take, CancellationToken cancellationToken = default);
}
