namespace LsKeeperSamscan.Config;

using System.ComponentModel.DataAnnotations;

public class AphaConfig
{
    [Required]
    public required string BaseUrl { get; init; }

    [Required]
    public required string TokenUrl { get; init; }

    [Required]
    public required string ClientId { get; init; }

    [Required]
    public required string ClientSecret { get; init; }

    [Range(1, 1000)]
    public int RateLimitPerSecond { get; init; } = 10;
}
