namespace LsKeeperSamscan.Config;

using System.ComponentModel.DataAnnotations;

public class DataBridgeConfig
{
    [Required]
    public required string BaseUrl { get; init; }

    [Range(1, 10000)]
    public int PageSize { get; init; } = 100;
}
