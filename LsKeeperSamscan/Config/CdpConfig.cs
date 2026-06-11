namespace LsKeeperSamscan.Config;

public class CdpConfig
{
    /// <summary>
    /// CDP ephemeral API key for local development.
    /// When set, HTTP clients send x-api-key header instead of OAuth2 Bearer token.
    /// </summary>
    public string? ApiKey { get; init; }

    public bool UseApiKeyAuth => !string.IsNullOrEmpty(ApiKey);
}
