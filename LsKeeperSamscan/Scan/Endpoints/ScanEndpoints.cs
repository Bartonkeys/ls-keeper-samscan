using LsKeeperSamscan.Scan.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace LsKeeperSamscan.Scan.Endpoints;

[ExcludeFromCodeCoverage]
public static class ScanEndpoints
{
    public static RouteGroupBuilder MapScanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scan")
            .WithTags("Scan");

        group.MapPost(string.Empty, TriggerScan)
            .WithSummary("Trigger SAM scan")
            .WithDescription("Starts a batch scan of SAM holdings. Returns 202 if started, 409 if a scan is already running.");
        group.MapGet("/status", GetStatus)
            .WithSummary("Get scan status")
            .WithDescription("Returns the current state of the scan job including progress counts and pre-signed CSV download URL when complete.");

        return group;
    }

    private static Results<Accepted<object>, Conflict<object>> TriggerScan(
        [FromServices] IScanJob scanJob)
    {
        if (!scanJob.TryStart())
        {
            return TypedResults.Conflict<object>(new { message = "Scan already in progress" });
        }

        return TypedResults.Accepted("/api/scan/status", (object)new
        {
            message = "Scan started",
            startedAt = DateTime.UtcNow
        });
    }

    private static Ok<object> GetStatus([FromServices] IScanJob scanJob)
    {
        var status = scanJob.GetStatus();
        return TypedResults.Ok<object>(new
        {
            state = status.State.ToString().ToLowerInvariant(),
            startedAt = status.StartedAt,
            completedAt = status.CompletedAt,
            totalHoldings = status.TotalHoldings,
            successCount = status.SuccessCount,
            errorCount = status.ErrorCount,
            presignedUrl = status.PresignedUrl,
            errorMessage = status.ErrorMessage
        });
    }
}
