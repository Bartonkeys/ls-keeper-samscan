using System.Globalization;
using CsvHelper;
using LsKeeperSamscan.Scan.Models;

namespace LsKeeperSamscan.Scan.Services;

public class CsvExportService : ICsvExportService
{
    public MemoryStream WriteCsv(IEnumerable<HoldingSummary> rows)
    {
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(rows);
        }

        stream.Position = 0;
        return stream;
    }
}
