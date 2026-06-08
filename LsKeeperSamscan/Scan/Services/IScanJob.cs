using LsKeeperSamscan.Scan.Models;

namespace LsKeeperSamscan.Scan.Services;

public interface IScanJob
{
    bool TryStart();
    ScanStatus GetStatus();
}
