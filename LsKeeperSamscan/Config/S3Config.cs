namespace LsKeeperSamscan.Config;

using System.ComponentModel.DataAnnotations;

public class S3Config
{
    [Required]
    public required string BucketName { get; init; }

    [Required]
    public required string Region { get; init; }

    public string Prefix { get; init; } = "samscan";

    public int PreSignedUrlTtlDays { get; init; } = 7;

    /// <summary>
    /// Optional override for local development (e.g. LocalStack/Floci endpoint).
    /// </summary>
    public string? ServiceUrl { get; init; }

    /// <summary>
    /// Required when using LocalStack/Floci (path-style access).
    /// </summary>
    public bool ForcePathStyle { get; init; }
}
