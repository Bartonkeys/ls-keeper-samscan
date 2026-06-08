using LsKeeperSamscan.Scan.Models.Apha;

namespace LsKeeperSamscan.Clients.Apha;

public interface IAphaClient
{
    Task<(AphaHoldingData? Holding, long ElapsedMs)> GetHoldingAsync(
        string countyId, string parishId, string holdingId, CancellationToken cancellationToken = default);

    Task<(AphaLocationData? Location, long ElapsedMs)> GetLocationAsync(
        string locationId, CancellationToken cancellationToken = default);

    Task<(AphaCustomerData? Customer, long ElapsedMs)> FindCustomerAsync(
        string customerId, CancellationToken cancellationToken = default);
}
