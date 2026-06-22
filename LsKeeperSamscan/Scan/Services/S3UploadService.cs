using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using LsKeeperSamscan.Config;
using Microsoft.Extensions.Options;

namespace LsKeeperSamscan.Scan.Services;

public class S3UploadService : IS3UploadService, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Config _config;
    private readonly ILogger<S3UploadService> _logger;

    public S3UploadService(IOptions<S3Config> options, ILogger<S3UploadService> logger)
    {
        _config = options.Value;
        _logger = logger;

        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_config.Region)
        };

        if (!string.IsNullOrEmpty(_config.ServiceUrl))
        {
            s3Config.ServiceURL = _config.ServiceUrl;
            s3Config.ForcePathStyle = _config.ForcePathStyle;
        }

        _s3Client = new AmazonS3Client(s3Config);
    }

    public async Task<string> UploadAndGetPresignedUrlAsync(Stream content, string objectKey, CancellationToken cancellationToken = default)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _config.BucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = "text/csv"
        };

        _logger.LogInformation("Uploading CSV to S3 bucket={Bucket} key={Key}", _config.BucketName, objectKey);
        await _s3Client.PutObjectAsync(putRequest, cancellationToken);
        _logger.LogInformation("S3 upload complete: {Key}", objectKey);

        var presignedRequest = new GetPreSignedUrlRequest
        {
            BucketName = _config.BucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddDays(_config.PreSignedUrlTtlDays),
            Verb = HttpVerb.GET
        };

        var presignedUrl = _s3Client.GetPreSignedURL(presignedRequest);
        _logger.LogInformation("Generated pre-signed URL (TTL={TtlDays} days): {Url}", _config.PreSignedUrlTtlDays, presignedUrl);

        return presignedUrl;
    }

    public void Dispose()
    {
        _s3Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
