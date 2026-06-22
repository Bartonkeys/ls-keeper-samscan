namespace LsKeeperSamscan.Scan.Services;

public interface IS3UploadService
{
    Task<string> UploadAndGetPresignedUrlAsync(Stream content, string objectKey, CancellationToken cancellationToken = default);
}
