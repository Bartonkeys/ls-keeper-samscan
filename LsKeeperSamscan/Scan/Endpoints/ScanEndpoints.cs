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

        group.MapPost(string.Empty, TriggerScan);
        group.MapGet("/status", GetStatus);

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
