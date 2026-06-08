using LsKeeperSamscan.Clients.Apha;
using LsKeeperSamscan.Clients.DataBridge;
using LsKeeperSamscan.Config;
using LsKeeperSamscan.Scan.Endpoints;
using LsKeeperSamscan.Scan.Services;
using LsKeeperSamscan.Utils;
using LsKeeperSamscan.Utils.Http;
using LsKeeperSamscan.Utils.Mongo;
using System.Diagnostics.CodeAnalysis;
using LsKeeperSamscan.Utils.Logging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;
using MongoDB.Driver.Authentication.AWS;
using Serilog;

var app = BuildApp(args);
await app.RunAsync();

[ExcludeFromCodeCoverage]
static WebApplication BuildApp(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureHost(builder);
    ConfigureServices(builder);

    var app = builder.Build();

    ConfigureMiddleware(app);
    ConfigureEndpoints(app);

    return app;
}

[ExcludeFromCodeCoverage]
static void ConfigureHost(WebApplicationBuilder builder)
{
    builder.Host.UseSerilog(CdpLogging.Configuration);
}

[ExcludeFromCodeCoverage]
static void ConfigureServices(WebApplicationBuilder builder)
{
    var services = builder.Services;
    var configuration = builder.Configuration;

    // Trust material must be loaded before anything creates outbound connections.
    services.LoadCustomTrustStoreFromEnvironment();

    services.AddProblemDetails();
    services.AddValidation();

    services.AddHttpContextAccessor();

    ConfigureHeaderPropagation(services, configuration);
    ConfigureHttpClients(services);
    ConfigureMongo(services, configuration);
    ConfigureOptions(services, configuration);

    services.AddHealthChecks();

    // App services
    services.AddSingleton<ICsvExportService, CsvExportService>();
    services.AddSingleton<IS3UploadService, S3UploadService>();
    services.AddSingleton<IScanJob, ScanJob>();
}

[ExcludeFromCodeCoverage]
static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
{
    services
        .AddOptions<AphaConfig>()
        .Bind(configuration.GetRequiredSection("Apha"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services
        .AddOptions<DataBridgeConfig>()
        .Bind(configuration.GetRequiredSection("DataBridge"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services
        .AddOptions<S3Config>()
        .Bind(configuration.GetRequiredSection("S3"))
        .ValidateDataAnnotations()
        .ValidateOnStart();
}

[ExcludeFromCodeCoverage]
static void ConfigureHeaderPropagation(IServiceCollection services, IConfiguration configuration)
{
    var traceHeader = configuration.GetValue<string>("TraceHeader");

    services.AddHeaderPropagation(options =>
    {
        if (!string.IsNullOrWhiteSpace(traceHeader))
        {
            options.Headers.Add(traceHeader);
        }
    });
}

[ExcludeFromCodeCoverage]
static void ConfigureHttpClients(IServiceCollection services)
{
    services.AddTransient<ProxyHttpMessageHandler>();

    services.AddHttpClientWithTracing<IAphaTokenProvider, AphaTokenProvider>();
    services.AddHttpClientWithTracing<IAphaClient, AphaClient>();
    services.AddHttpClientWithTracing<IDataBridgeClient, DataBridgeClient>();
}

[ExcludeFromCodeCoverage]
static void ConfigureMongo(IServiceCollection services, IConfiguration configuration)
{

    MongoExtensions.Register();
    MongoConventions.Register();

    services
        .AddOptions<MongoConfig>()
        .Bind(configuration.GetRequiredSection("Mongo"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
}

[ExcludeFromCodeCoverage]
static void ConfigureMiddleware(WebApplication app)
{
    app.UseSerilogRequestLogging();

    app.UseHeaderPropagation();
}

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplication app)
{
    app.MapHealthChecks("/health", new HealthCheckOptions());

    app.MapScanEndpoints();
}
