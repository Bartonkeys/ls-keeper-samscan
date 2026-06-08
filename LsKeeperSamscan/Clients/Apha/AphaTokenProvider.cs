using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LsKeeperSamscan.Config;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Clients.Apha;

public class AphaTokenProvider(
    HttpClient httpClient,
    IOptions<AphaConfig> options,
    ILogger<AphaTokenProvider> logger) : IAphaTokenProvider
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            var config = options.Value;
            logger.LogInformation("Requesting new APHA OAuth2 token from {TokenUrl}", config.TokenUrl);

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret
            });

            var response = await httpClient.PostAsync(config.TokenUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
            if (tokenResponse?.AccessToken is null)
            {
                throw new InvalidOperationException("APHA token response did not contain an access token.");
            }

            _cachedToken = tokenResponse.AccessToken;
            // Expire 60 seconds early to avoid edge-case failures
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            logger.LogInformation("APHA OAuth2 token acquired, expires in {ExpiresIn}s", tokenResponse.ExpiresIn);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
