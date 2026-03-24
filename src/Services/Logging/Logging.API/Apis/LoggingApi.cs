using Asp.Versioning;
using JiraLite.Share.Common;
using Logging.Application.Dtos;
using Logging.Application.Feature.Logs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logging.API.Apis;

public static class LoggingApi
{
    public static IEndpointRouteBuilder MapLoggingApi(this IEndpointRouteBuilder builder)
    {
        var vApi = builder.NewVersionedApi("Logging");
        var v1 = vApi.MapGroup("api/v{version:apiVersion}/logs")
            .HasApiVersion(new ApiVersion(1, 0));

        v1.MapGet("/issues/{issueId}", GetIssueLogs)
            .WithName("GetIssueLogs")
            .WithTags("Activity Logs");

        v1.MapGet("/projects/{projectId}", GetProjectLogs)
            .WithName("GetProjectLogs")
            .WithTags("Activity Logs");

        v1.MapGet("/users/{userId}", GetUserLogs)
            .WithName("GetUserLogs")
            .WithTags("Activity Logs");

        return builder;
    }

    private static async Task<IResult> GetIssueLogs(
        [FromRoute] string issueId,
        [AsParameters] PaginationRequest pagination,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var query = new GetIssueLogsQuery.Query(issueId, pagination.PageIndex, pagination.PageSize);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetProjectLogs(
        [FromRoute] string projectId,
        [AsParameters] PaginationRequest pagination,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var query = new GetProjectLogsQuery.Query(projectId, pagination.PageIndex, pagination.PageSize);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetUserLogs(
        [FromRoute] string userId,
        [AsParameters] PaginationRequest pagination,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var query = new GetUserLogQuery.Query(userId, pagination.PageIndex, pagination.PageSize);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }
        return Results.Ok(result.Value);
    }
}
