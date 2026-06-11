namespace LsKeeperSamscan.Config;

using System.ComponentModel.DataAnnotations;

public class DataBridgeConfig
{
    [Required]
    public required string BaseUrl { get; init; }

    [Range(1, 10000)]
    public int PageSize { get; init; } = 100;

    /// <summary>
    /// CDP ephemeral API key for local development.
    /// When set, requests include x-api-key header.
    /// </summary>
    public string? ApiKey { get; init; }
}
