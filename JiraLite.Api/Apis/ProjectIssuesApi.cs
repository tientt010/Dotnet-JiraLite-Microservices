
using JiraLite.Application.Interfaces;
using JiraLite.Authorization.Constants;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Issues;
using JiraLite.Share.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Api.Apis;

public static class ProjectIssuesApi
{
    public static RouteGroupBuilder MapProjectIssuesApi(this RouteGroupBuilder group)
    {
        var issues = group.MapGroup("/projects/{projectId:guid}/issues").WithTags("Project Issues");

        issues.MapGet("/", GetProjectIssuesAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectMember);

        issues.MapPost("/", CreateIssueAsync)
            .RequireAuthorization(PolicyNames.ProjectManager);

        return issues;
    }
    private static async Task<Results<Ok<PaginationResponse<IssueInfoDto>>, BadRequest<Error>>> GetProjectIssuesAsync(
        [FromRoute] Guid projectId,
        [FromQuery] IssueStatus? status,
        [FromQuery] Guid? assigneeId,
        [FromQuery] string? search,
        [AsParameters] PaginationRequest pagination,
        [FromServices] IProjectIssuesServices projectIssuesService,
        CancellationToken cancellationToken = default)
    {
        var result = await projectIssuesService.GetProjectIssuesAsync(projectId, status, assigneeId, search, pagination, cancellationToken);
        if (result.IsFailure)
        {
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Created<CreateIssueResponse>, NotFound<Error>, BadRequest<Error>>> CreateIssueAsync(
        [FromRoute] Guid projectId,
        [FromBody] CreateIssueRequest request,
        [FromServices] IProjectIssuesServices projectIssuesService,
        CancellationToken cancellationToken = default)
    {
        var result = await projectIssuesService.CreateIssueAsync(projectId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == ProjectErrors.ProjectNotFound)
                return TypedResults.NotFound(result.Error);
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Created($"/api/v1/issues/{result.Value!.Id}", result.Value);
    }
}
