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
}
