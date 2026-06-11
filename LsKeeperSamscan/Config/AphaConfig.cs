namespace LsKeeperSamscan.Config;

using System.ComponentModel.DataAnnotations;

public class AphaConfig
{
    [Required]
    public required string BaseUrl { get; init; }

    /// <summary>
    /// OAuth2 token endpoint. Required when using OAuth2 auth (production).
    /// </summary>
    public string? TokenUrl { get; init; }

    /// <summary>
    /// OAuth2 client ID. Required when using OAuth2 auth (production).
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// OAuth2 client secret. Required when using OAuth2 auth (production).
    /// </summary>
    public string? ClientSecret { get; init; }
}
