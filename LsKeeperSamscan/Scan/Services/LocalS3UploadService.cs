using LsKeeperSamscan.Config;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Scan.Services;

/// <summary>
/// Development-only implementation of <see cref="IS3UploadService"/> that writes
/// the CSV to a local directory instead of uploading to S3.
/// </summary>
public class LocalS3UploadService(IOptions<S3Config> options, ILogger<LocalS3UploadService> logger)
    : IS3UploadService
{
    public async Task<string> UploadAndGetPresignedUrlAsync(Stream stream, string objectKey, CancellationToken cancellationToken = default)
    {
        var outputDir = options.Value.LocalOutputPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "samscan-output");

        Directory.CreateDirectory(outputDir);

        // Flatten the object key (e.g. "samscan/2024-01-01/120000.csv") into a safe filename
        var fileName = objectKey.Replace('/', '_');
        var filePath = Path.GetFullPath(Path.Combine(outputDir, fileName));

        stream.Position = 0;
        await using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream);

        logger.LogInformation("LocalS3UploadService: CSV written to {FilePath}", filePath);

        return filePath;
    }
}
