using System.Net;
using LsKeeperSamscan.Scan.Models;
using LsKeeperSamscan.Scan.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace LsKeeperSamscan.Test.Scan;

public class ScanEndpointsTest
{
    [Fact]
    public async Task Post_scan_returns_accepted_when_idle()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new ScanTestApplicationFactory();

        factory.MockScanJob.TryStart().Returns(true);

        using var client = factory.CreateClient();
        var response = await client.PostAsync("/api/scan", null, cancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Post_scan_returns_conflict_when_already_running()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new ScanTestApplicationFactory();

        factory.MockScanJob.TryStart().Returns(false);

        using var client = factory.CreateClient();
        var response = await client.PostAsync("/api/scan", null, cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Get_status_returns_ok()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new ScanTestApplicationFactory();

        factory.MockScanJob.GetStatus().Returns(new ScanStatus
        {
            State = ScanState.Idle
        });

        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/scan/status", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_is_available()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new ScanTestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class ScanTestApplicationFactory : WebApplicationFactory<Program>
    {
        public readonly IScanJob MockScanJob = Substitute.For<IScanJob>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Apha:BaseUrl"] = "https://test.example.com",
                    ["Apha:TokenUrl"] = "https://test.example.com/oauth2/token",
                    ["Apha:ClientId"] = "test-client-id",
                    ["Apha:ClientSecret"] = "test-client-secret",
                    ["DataBridge:BaseUrl"] = "https://test.example.com",
                    ["DataBridge:PageSize"] = "10",
                    ["S3:BucketName"] = "test-bucket",
                    ["S3:Region"] = "eu-west-2",
                    ["Mongo:DatabaseUri"] = "mongodb://localhost:27017",
                    ["Mongo:DatabaseName"] = "test-db"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IScanJob>();
                services.AddSingleton(MockScanJob);
            });
        }
    }
}
