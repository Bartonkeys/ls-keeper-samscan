namespace LsKeeperSamscan.Config;

using System.ComponentModel.DataAnnotations;

public class DataBridgeConfig
{
    [Required]
    public required string BaseUrl { get; init; }

    [Range(1, 10000)]
    public int PageSize { get; init; } = 100;

    /// <summary>
    /// Authorization key for the data bridge (sent as "Authorization: ApiKey {value}").
    /// </summary>
    public string? AuthKey { get; init; }

    /// <summary>
    /// When set, caps the number of SAM holdings processed per scan run.
    /// Useful for testing end-to-end without processing the full dataset.
    /// Leave null (or omit from config) for a full run.
    /// </summary>
    public int? MaxHoldings { get; init; }
}
