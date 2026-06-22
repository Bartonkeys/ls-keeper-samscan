namespace LsKeeperSamscan.Scan.Models;

public enum ScanState
{
    Idle,
    Running,
    Completed,
    Failed
}

public class ScanStatus
{
    public ScanState State { get; set; } = ScanState.Idle;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalHoldings { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public string? PresignedUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
