using JiraLite.Application.Interfaces;
using JiraLite.Authorization.Constants;
using JiraLite.Authorization.Services;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Issues;
using JiraLite.Share.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Api.Apis;

public static class IssuesApi
{
    public static RouteGroupBuilder MapIssuesApi(this RouteGroupBuilder group)
    {
        var issues = group.MapGroup("/issues").WithTags("Issues");
        issues.MapGet("/", GetIssuesAsync)
            .RequireAuthorization();

        issues.MapGet("/{issueId:guid}", GetIssueByIdAsync)
            .RequireAuthorization(PolicyNames.ProjectMember);

        issues.MapPut("/{issueId:guid}", UpdateIssueAsync)
            .RequireAuthorization(PolicyNames.ProjectManagerOrAssignee);

        issues.MapPatch("/{issueId:guid}/status", UpdateStatusAsync)
            .RequireAuthorization(PolicyNames.ProjectManagerOrAssignee);

        issues.MapPatch("/{issueId:guid}/priority", UpdatePriorityAsync)
            .RequireAuthorization(PolicyNames.ProjectManagerOrAssignee);

        issues.MapPatch("/{issueId:guid}/assignee", UpdateAssigneeAsync)
            .RequireAuthorization(PolicyNames.ProjectManager);



        issues.MapDelete("/{issueId:guid}", RejectIssueAsync)
            .RequireAuthorization(PolicyNames.ProjectManager);

        return issues;
    }

    private static async Task<Results<Ok, BadRequest<Error>, NotFound<Error>, ForbidHttpResult>> UpdateAssigneeAsync(
        [FromRoute] Guid issueId,
        [FromBody] UpdateIssueAssigneeRequest request,
        [FromServices] IIdentityService identityService,
        [FromServices] IIssueService issueService,
        CancellationToken cancellationToken = default)
    {
        if (!identityService.TryGetUserId(out var userId))
        {
            return TypedResults.BadRequest(UserErrors.InvalidAccessToken);
        }
        var result = await issueService.UpdateIssueAssigneeAsync(issueId, userId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == IssueErrors.IssueNotFound || result.Error == ProjectErrors.ProjectNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            if (result.Error == IssueErrors.PermissionDenied)
            {
                return TypedResults.Forbid();
            }
            return TypedResults.BadRequest(result.Error);
        }

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<Error>, NotFound<Error>>> UpdateStatusAsync(
        [FromRoute] Guid issueId,
        [FromBody] UpdateIssueStatusRequest request,
        [FromServices] IIdentityService identityService,
        [FromServices] IIssueService issueService,
        CancellationToken cancellationToken = default)
    {
        if (!identityService.TryGetUserId(out var userId))
        {
            return TypedResults.BadRequest(UserErrors.InvalidAccessToken);
        }
        var result = await issueService.UpdateIssueStatusAsync(issueId, userId, request.Status, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == IssueErrors.IssueNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<Error>, NotFound<Error>>> UpdatePriorityAsync(
        [FromRoute] Guid issueId,
        [FromBody] UpdateIssuePriorityRequest request,
        [FromServices] IIdentityService identityService,
        [FromServices] IIssueService issueService,
        CancellationToken cancellationToken = default)
    {
        if (!identityService.TryGetUserId(out var userId))
        {
            return TypedResults.BadRequest(UserErrors.InvalidAccessToken);
        }
        var result = await issueService.UpdateIssuePriorityAsync(issueId, userId, request.Priority, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == IssueErrors.IssueNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<PaginationResponse<IssueInfoDto>>, BadRequest<Error>>> GetIssuesAsync(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? assigneeId,
        [FromQuery] IssueStatus? status,
        [FromQuery] IssuePriority? priority,
        [FromQuery] string? searchStr,
        [AsParameters] PaginationRequest pagination,
        [FromServices] IIssueService issuesService,
        CancellationToken cancellationToken = default)
    {
        var result = await issuesService.GetIssuesAsync(projectId, assigneeId, status, priority, searchStr, pagination, cancellationToken);
        if (result.IsFailure)
        {
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<IssueDetailDto>, BadRequest<Error>, NotFound<Error>>> GetIssueByIdAsync(
        [FromRoute] Guid issueId,
        [FromServices] IIssueService issuesService,
        CancellationToken cancellationToken = default)
    {
        var result = await issuesService.GetIssueByIdAsync(issueId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == IssueErrors.IssueNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<IssueInfoDto>, BadRequest<Error>, NotFound<Error>>> UpdateIssueAsync(
        [FromRoute] Guid issueId,
        [FromBody] UpdateIssueRequest request,
        [FromServices] IIssueService issuesService,
        [FromServices] IIdentityService identityService,
        CancellationToken cancellationToken = default)
    {
        if (!identityService.TryGetUserId(out var userId))
        {
            return TypedResults.BadRequest(UserErrors.InvalidAccessToken);
        }
        var result = await issuesService.UpdateIssueAsync(issueId, userId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == IssueErrors.IssueNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }


    private static async Task<Results<Ok, BadRequest<Error>, NotFound<Error>>> RejectIssueAsync(
        [FromRoute] Guid issueId,
        [FromServices] IIdentityService identityService,
        [FromServices] IIssueService issueService,
        CancellationToken cancellationToken = default)
    {
        if (!identityService.TryGetUserId(out var userId))
        {
            return TypedResults.BadRequest(UserErrors.InvalidAccessToken);
        }

        var result = await issueService.RejectIssueAsync(issueId, userId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == IssueErrors.IssueNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok();
    }
}
