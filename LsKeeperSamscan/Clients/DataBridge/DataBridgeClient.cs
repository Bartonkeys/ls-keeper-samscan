using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LsKeeperSamscan.Config;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Clients.DataBridge;

public class DataBridgeClient(
    HttpClient httpClient,
    IOptions<DataBridgeConfig> options,
    ILogger<DataBridgeClient> logger) : IDataBridgeClient
{
    public async Task<int> GetSamHoldingsCountAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{options.Value.BaseUrl}/api/Holdings/sam-list?skip=0&take=0";
        logger.LogInformation("Fetching SAM holdings count from {Url}", url);

        var response = await httpClient.GetFromJsonAsync<SamHoldingsListResponse>(url, cancellationToken);
        var count = response?.TotalCount ?? 0;

        logger.LogInformation("SAM holdings total count: {Count}", count);
        return count;
    }

    public async Task<List<string>> ListSamHoldingsAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var url = $"{options.Value.BaseUrl}/api/Holdings/sam-list?skip={skip}&take={take}";
        logger.LogInformation("Fetching SAM holdings page skip={Skip} take={Take} from {Url}", skip, take, url);

        var response = await httpClient.GetFromJsonAsync<SamHoldingsListResponse>(url, cancellationToken);
        var cphs = response?.Data ?? [];

        logger.LogInformation("Fetched {Count} SAM holdings (skip={Skip})", cphs.Count, skip);
        return cphs;
    }

    private class SamHoldingsListResponse
    {
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("data")]
        public List<string> Data { get; set; } = [];
    }
}
