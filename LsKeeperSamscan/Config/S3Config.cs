namespace LsKeeperSamscan.Config;

using System.ComponentModel.DataAnnotations;

public class S3Config
{
    [Required]
    public required string BucketName { get; init; }

    [Required]
    public required string Region { get; init; }

    public string? Prefix { get; init; }

    public int PreSignedUrlTtlDays { get; init; } = 7;

    /// <summary>
    /// Optional override for the S3 service endpoint (e.g. LocalStack).
    /// </summary>
    public string? ServiceUrl { get; init; }

    public bool ForcePathStyle { get; init; }

    public string? AuthKey { get; init; }

    /// <summary>
    /// Development only: when set, the CSV is written to this local directory
    /// instead of being uploaded to S3. See <see cref="Scan.Services.LocalS3UploadService"/>.
    /// </summary>
    public string? LocalOutputPath { get; init; }
}
