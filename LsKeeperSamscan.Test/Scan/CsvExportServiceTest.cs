using LsKeeperSamscan.Scan.Models;
using LsKeeperSamscan.Scan.Services;

namespace LsKeeperSamscan.Test.Scan;

public class CsvExportServiceTest
{
    [Fact]
    public void WriteCsv_produces_valid_csv_with_headers_and_rows()
    {
        var service = new CsvExportService();
        var rows = new List<HoldingSummary>
        {
            new()
            {
                Cph = "10/011/0011",
                CphType = "Parish",
                LocationId = "L12345",
                LocationName = "Test Farm",
                LocationStreet = "123 Farm Lane",
                LocationTown = "Farmville",
                LocationPostcode = "FA1 1RM",
                CustomerTitle = "Mr",
                CustomerFirstName = "John",
                CustomerLastName = "Doe",
                AphaHoldingMs = 150,
                AphaLocationMs = 200,
                AphaCustomerMs = 100
            },
            new()
            {
                Cph = "52/243/4001",
                EnrichmentError = "APHA holding not found",
                AphaHoldingMs = 50
            }
        };

        using var stream = service.WriteCsv(rows);
        using var reader = new StreamReader(stream);
        var csv = reader.ReadToEnd();

        Assert.Contains("Cph", csv);
        Assert.Contains("CphType", csv);
        Assert.Contains("EnrichmentError", csv);
        Assert.Contains("10/011/0011", csv);
        Assert.Contains("52/243/4001", csv);
        Assert.Contains("APHA holding not found", csv);
        Assert.Contains("Test Farm", csv);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length); // header + 2 data rows
    }

    [Fact]
    public void WriteCsv_empty_rows_produces_header_only()
    {
        var service = new CsvExportService();
        var rows = new List<HoldingSummary>();

        using var stream = service.WriteCsv(rows);
        using var reader = new StreamReader(stream);
        var csv = reader.ReadToEnd();

        Assert.Contains("Cph", csv);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines); // header only
    }
}
