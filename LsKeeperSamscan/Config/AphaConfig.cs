namespace LsKeeperSamscan.Config;

using System.ComponentModel.DataAnnotations;

public class AphaConfig : IValidatableObject
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

    /// <summary>
    /// CDP ephemeral API key for local development.
    /// When set, requests use x-api-key header instead of OAuth2 Bearer token.
    /// </summary>
    public string? ApiKey { get; init; }

    public bool UseApiKeyAuth => !string.IsNullOrEmpty(ApiKey);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!UseApiKeyAuth)
        {
            if (string.IsNullOrEmpty(TokenUrl))
                yield return new ValidationResult("TokenUrl is required when ApiKey is not set.", [nameof(TokenUrl)]);
            if (string.IsNullOrEmpty(ClientId))
                yield return new ValidationResult("ClientId is required when ApiKey is not set.", [nameof(ClientId)]);
            if (string.IsNullOrEmpty(ClientSecret))
                yield return new ValidationResult("ClientSecret is required when ApiKey is not set.", [nameof(ClientSecret)]);
        }
    }
}
