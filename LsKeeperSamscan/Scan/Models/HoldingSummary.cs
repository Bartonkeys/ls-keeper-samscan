namespace LsKeeperSamscan.Scan.Models;

/// <summary>
/// Flattened row model written to the CSV export.
/// </summary>
public class HoldingSummary
{
    public string Cph { get; set; } = string.Empty;
    public string? CphType { get; set; }
    public string? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? LocationStreet { get; set; }
    public string? LocationLocality { get; set; }
    public string? LocationTown { get; set; }
    public string? LocationCounty { get; set; }
    public string? LocationPostcode { get; set; }
    public string? LocationCountryCode { get; set; }
    public string? OsMapReference { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerTitle { get; set; }
    public string? CustomerFirstName { get; set; }
    public string? CustomerLastName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public int CommoditiesCount { get; set; }
    public int FacilitiesCount { get; set; }
    public string? EnrichmentError { get; set; }
    public long AphaHoldingMs { get; set; }
    public long AphaLocationMs { get; set; }
    public long AphaCustomerMs { get; set; }
}
