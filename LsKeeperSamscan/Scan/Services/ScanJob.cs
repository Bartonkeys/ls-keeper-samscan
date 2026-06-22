using System.Diagnostics;
using LsKeeperSamscan.Clients.Apha;
using LsKeeperSamscan.Clients.DataBridge;
using LsKeeperSamscan.Config;
using LsKeeperSamscan.Scan.Models;
using LsKeeperSamscan.Scan.Models.Apha;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Scan.Services;

public class ScanJob(
    IDataBridgeClient dataBridgeClient,
    IAphaClient aphaClient,
    ICsvExportService csvExportService,
    IS3UploadService s3UploadService,
    IOptions<DataBridgeConfig> dataBridgeOptions,
    IOptions<S3Config> s3Options,
    ILogger<ScanJob> logger) : IScanJob
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private ScanStatus _status = new();

    public bool TryStart()
    {
        if (!_semaphore.Wait(0))
        {
            return false;
        }

        // Fire and forget — release semaphore when done
        _ = Task.Run(async () =>
        {
            try
            {
                await ExecuteScanAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        });

        return true;
    }

    public ScanStatus GetStatus() => _status;

    private async Task ExecuteScanAsync()
    {
        var sw = Stopwatch.StartNew();
        _status = new ScanStatus
        {
            State = ScanState.Running,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            logger.LogInformation("SAM scan started at {StartedAt}", _status.StartedAt);

            // Step 1: Get total count
            var totalCount = await dataBridgeClient.GetSamHoldingsCountAsync();
            _status.TotalHoldings = totalCount;

            if (dataBridgeOptions.Value.MaxHoldings.HasValue)
            {
                totalCount = Math.Min(totalCount, dataBridgeOptions.Value.MaxHoldings.Value);
                logger.LogWarning("SAM scan: MaxHoldings cap active, processing {CappedCount} of {TotalCount} holdings",
                    totalCount, _status.TotalHoldings);
            }
            else
            {
                logger.LogInformation("SAM scan: {TotalCount} holdings to process", totalCount);
            }

            // Step 2 & 3: Page through holdings and enrich each
            var pageSize = dataBridgeOptions.Value.PageSize;
            var allRows = new List<HoldingSummary>();
            var successCount = 0;
            var errorCount = 0;

            for (var skip = 0; skip < totalCount; skip += pageSize)
            {
                var pageSw = Stopwatch.StartNew();
                var cphs = await dataBridgeClient.ListSamHoldingsAsync(skip, pageSize);
                logger.LogInformation("SAM scan: fetched page skip={Skip} count={Count} in {ElapsedMs}ms",
                    skip, cphs.Count, pageSw.ElapsedMilliseconds);

                foreach (var cph in cphs)
                {
                    var row = await EnrichHoldingAsync(cph);
                    allRows.Add(row);

                    if (row.EnrichmentError is null)
                        successCount++;
                    else
                        errorCount++;
                }
            }

            _status.SuccessCount = successCount;
            _status.ErrorCount = errorCount;

            // Step 4: Write CSV
            logger.LogInformation("SAM scan: writing CSV for {RowCount} rows", allRows.Count);
            using var csvStream = csvExportService.WriteCsv(allRows);
            logger.LogInformation("SAM scan: CSV written, size={SizeBytes} bytes", csvStream.Length);

            // Step 5: Upload to S3
            var prefix = s3Options.Value.Prefix;
            var objectKey = $"{prefix}/{DateTime.UtcNow:yyyy-MM-dd}/{DateTime.UtcNow:HHmmss}.csv";
            var presignedUrl = await s3UploadService.UploadAndGetPresignedUrlAsync(csvStream, objectKey);

            // Step 6: Log pre-signed URL
            _status.PresignedUrl = presignedUrl;
            _status.State = ScanState.Completed;
            _status.CompletedAt = DateTime.UtcNow;

            sw.Stop();
            logger.LogInformation(
                "SAM scan complete. Total={Total}, Success={Success}, Errors={Errors}, Duration={DurationMs}ms. Download CSV: {Url}",
                totalCount, successCount, errorCount, sw.ElapsedMilliseconds, presignedUrl);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _status.State = ScanState.Failed;
            _status.CompletedAt = DateTime.UtcNow;
            _status.ErrorMessage = ex.Message;

            logger.LogError(ex, "SAM scan failed after {DurationMs}ms", sw.ElapsedMilliseconds);
        }
    }

    private async Task<HoldingSummary> EnrichHoldingAsync(string cph)
    {
        var row = new HoldingSummary { Cph = cph };

        try
        {
            var parts = cph.Split('/');
            if (parts.Length != 3)
            {
                row.EnrichmentError = $"Invalid CPH format: {cph}";
                return row;
            }

            var (countyId, parishId, holdingId) = (parts[0], parts[1], parts[2]);

            // Get holding
            var (holding, holdingMs) = await aphaClient.GetHoldingAsync(countyId, parishId, holdingId);
            row.AphaHoldingMs = holdingMs;

            if (holding is null)
            {
                row.EnrichmentError = "APHA holding not found";
                return row;
            }

            row.CphType = holding.CphType;

            // Get location
            var locationId = holding.Relationships?.Location?.Data?.Id;
            row.LocationId = locationId;

            if (locationId is not null)
            {
                var (location, locationMs) = await aphaClient.GetLocationAsync(locationId);
                row.AphaLocationMs = locationMs;

                if (location is not null)
                {
                    row.LocationName = location.Name;
                    row.LocationStreet = location.Address?.Street;
                    row.LocationLocality = location.Address?.Locality;
                    row.LocationTown = location.Address?.Town;
                    row.LocationCounty = location.Address?.AdministrativeAreaCounty;
                    row.LocationPostcode = location.Address?.Postcode;
                    row.LocationCountryCode = location.Address?.CountryCode;
                    row.OsMapReference = location.OsMapReference;
                    row.CommoditiesCount = location.LivestockUnits?.Count ?? 0;
                    row.FacilitiesCount = location.Facilities?.Count ?? 0;
                }
            }

            // Get customer (CPH holder)
            var customerId = holding.Relationships?.CphHolder?.Data?.Id;
            row.CustomerId = customerId;

            if (customerId is not null)
            {
                var (customer, customerMs) = await aphaClient.FindCustomerAsync(customerId);
                row.AphaCustomerMs = customerMs;

                if (customer is not null)
                {
                    row.CustomerTitle = customer.Title;
                    row.CustomerFirstName = customer.FirstName;
                    row.CustomerLastName = customer.LastName;
                    row.CustomerEmail = customer.ContactDetails?
                        .FirstOrDefault(c => c.Type == "email")?.EmailAddress;
                    row.CustomerPhone = customer.ContactDetails?
                        .FirstOrDefault(c => c.Type is "mobile" or "landline")?.PhoneNumber;
                }
            }
        }
        catch (Exception ex)
        {
            row.EnrichmentError = ex.Message;
            logger.LogWarning(ex, "Enrichment failed for CPH {Cph}", cph);
        }

        return row;
    }
}
