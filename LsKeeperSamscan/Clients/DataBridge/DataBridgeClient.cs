using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LsKeeperSamscan.Config;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Clients.DataBridge;

public class DataBridgeClient(
    HttpClient httpClient,
    IOptions<DataBridgeConfig> options,
    ILogger<DataBridgeClient> logger) : IDataBridgeClient
{
    private const string CollectionName = "sam_cph_holdings";

    public async Task<int> GetSamHoldingsCountAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{options.Value.BaseUrl}/api/query/{CollectionName}?$top=0&$count=true";
        logger.LogInformation("Fetching SAM holdings count from {Url}", url);

        var response = await httpClient.GetFromJsonAsync<ODataQueryResponse>(url, cancellationToken);
        var count = (int)(response?.TotalCount ?? 0);

        logger.LogInformation("SAM holdings total count: {Count}", count);
        return count;
    }

    public async Task<List<string>> ListSamHoldingsAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var url = $"{options.Value.BaseUrl}/api/query/{CollectionName}?$skip={skip}&$top={take}&$count=false&$select=CPH";
        logger.LogInformation("Fetching SAM holdings page skip={Skip} take={Take} from {Url}", skip, take, url);

        var response = await httpClient.GetFromJsonAsync<ODataQueryResponse>(url, cancellationToken);
        var cphs = ExtractCphValues(response?.Data);

        logger.LogInformation("Fetched {Count} SAM holdings (skip={Skip})", cphs.Count, skip);
        return cphs;
    }

    private static List<string> ExtractCphValues(IReadOnlyList<Dictionary<string, JsonElement>>? data)
    {
        if (data is null) return [];

        return data
            .Where(d => d.ContainsKey("CPH"))
            .Select(d => d["CPH"].GetString() ?? string.Empty)
            .Where(cph => !string.IsNullOrEmpty(cph))
            .ToList();
    }

    private class ODataQueryResponse
    {
        [JsonPropertyName("collectionName")]
        public string? CollectionName { get; set; }

        [JsonPropertyName("data")]
        public IReadOnlyList<Dictionary<string, JsonElement>>? Data { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("totalCount")]
        public long? TotalCount { get; set; }
    }
}
