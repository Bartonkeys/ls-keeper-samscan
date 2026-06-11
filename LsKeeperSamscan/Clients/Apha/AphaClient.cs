using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LsKeeperSamscan.Config;
using LsKeeperSamscan.Scan.Models.Apha;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Clients.Apha;

public class AphaClient(
    HttpClient httpClient,
    IAphaTokenProvider tokenProvider,
    IOptions<AphaConfig> options,
    ILogger<AphaClient> logger) : IAphaClient
{
    public async Task<(AphaHoldingData? Holding, long ElapsedMs)> GetHoldingAsync(
        string countyId, string parishId, string holdingId, CancellationToken cancellationToken = default)
    {
        var url = $"{options.Value.BaseUrl}/holdings/{countyId}/{parishId}/{holdingId}";
        var sw = Stopwatch.StartNew();

        try
        {
            var request = await CreateAuthedRequest(HttpMethod.Get, url, cancellationToken);
            var response = await httpClient.SendAsync(request, cancellationToken);

            sw.Stop();
            logger.LogInformation("APHA GET holding {Cph} responded {StatusCode} in {ElapsedMs}ms",
                $"{countyId}/{parishId}/{holdingId}", (int)response.StatusCode, sw.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                return (null, sw.ElapsedMilliseconds);
            }

            var body = await response.Content.ReadFromJsonAsync<AphaHoldingResponse>(cancellationToken: cancellationToken);
            return (body?.Data, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogWarning(ex, "APHA GET holding {Cph} failed after {ElapsedMs}ms",
                $"{countyId}/{parishId}/{holdingId}", sw.ElapsedMilliseconds);
            return (null, sw.ElapsedMilliseconds);
        }
    }

    public async Task<(AphaLocationData? Location, long ElapsedMs)> GetLocationAsync(
        string locationId, CancellationToken cancellationToken = default)
    {
        var url = $"{options.Value.BaseUrl}/locations/{locationId}";
        var sw = Stopwatch.StartNew();

        try
        {
            var request = await CreateAuthedRequest(HttpMethod.Get, url, cancellationToken);
            var response = await httpClient.SendAsync(request, cancellationToken);

            sw.Stop();
            logger.LogInformation("APHA GET location {LocationId} responded {StatusCode} in {ElapsedMs}ms",
                locationId, (int)response.StatusCode, sw.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                return (null, sw.ElapsedMilliseconds);
            }

            var body = await response.Content.ReadFromJsonAsync<AphaLocationResponse>(cancellationToken: cancellationToken);
            return (body?.Data, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogWarning(ex, "APHA GET location {LocationId} failed after {ElapsedMs}ms",
                locationId, sw.ElapsedMilliseconds);
            return (null, sw.ElapsedMilliseconds);
        }
    }

    public async Task<(AphaCustomerData? Customer, long ElapsedMs)> FindCustomerAsync(
        string customerId, CancellationToken cancellationToken = default)
    {
        var url = $"{options.Value.BaseUrl}/customers/find";
        var sw = Stopwatch.StartNew();

        try
        {
            var request = await CreateAuthedRequest(HttpMethod.Post, url, cancellationToken);
            request.Content = JsonContent.Create(new { ids = new[] { customerId } });

            var response = await httpClient.SendAsync(request, cancellationToken);

            sw.Stop();
            logger.LogInformation("APHA POST customers/find for {CustomerId} responded {StatusCode} in {ElapsedMs}ms",
                customerId, (int)response.StatusCode, sw.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                return (null, sw.ElapsedMilliseconds);
            }

            var body = await response.Content.ReadFromJsonAsync<AphaCustomerFindResponse>(cancellationToken: cancellationToken);
            var customer = body?.Data?.FirstOrDefault();
            return (customer, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogWarning(ex, "APHA POST customers/find for {CustomerId} failed after {ElapsedMs}ms",
                customerId, sw.ElapsedMilliseconds);
            return (null, sw.ElapsedMilliseconds);
        }
    }

    private async Task<HttpRequestMessage> CreateAuthedRequest(HttpMethod method, string url, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, url);

        if (options.Value.UseApiKeyAuth)
        {
            request.Headers.Add("x-api-key", options.Value.ApiKey);
        }
        else
        {
            var token = await tokenProvider.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }
}
