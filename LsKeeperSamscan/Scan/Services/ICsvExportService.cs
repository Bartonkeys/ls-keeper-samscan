using LsKeeperSamscan.Scan.Models;

namespace LsKeeperSamscan.Scan.Services;

public interface ICsvExportService
{
    MemoryStream WriteCsv(IEnumerable<HoldingSummary> rows);
}
