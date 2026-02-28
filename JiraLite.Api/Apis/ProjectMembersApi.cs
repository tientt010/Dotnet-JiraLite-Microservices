using System;
using JiraLite.Application.Interfaces;
using JiraLite.Authorization.Constants;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Projects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Api.Apis;

public static class ProjectMembersApi
{
    public static RouteGroupBuilder MapProjectMembersApi(this RouteGroupBuilder group)
    {
        var members = group.MapGroup("/projects/{projectId:guid}/members").WithTags("Project Members");

        members.MapGet("/", GetProjectMembersAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectMember);

        members.MapPost("/", AddProjectMemberAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);

        members.MapPatch("/{userId:guid}", UpdateMemberRoleAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);

        members.MapDelete("/{userId:guid}", RemoveProjectMemberAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);

        return members;
    }

    private static async Task<Ok<IReadOnlyList<ProjectMemberDto>>> GetProjectMembersAsync(
        [FromRoute] Guid projectId,
        [FromServices] IProjectMemberService projectService,
        CancellationToken cancellationToken = default)
    {
        var result = await projectService.GetProjectMembersAsync(projectId, cancellationToken);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok, NotFound<Error>, Conflict<Error>>> AddProjectMemberAsync(
        [FromRoute] Guid projectId,
        [FromBody] AddProjectMemberRequest request,
        [FromServices] IProjectMemberService projectService,
        CancellationToken cancellationToken = default)
    {
        var result = await projectService.AddProjectMemberAsync(projectId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == ProjectErrors.ProjectNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }

            if (result.Error == ProjectErrors.ProjectMemberAlreadyExists)
            {
                return TypedResults.Conflict(result.Error);
            }
        }
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound<Error>, BadRequest<Error>>> UpdateMemberRoleAsync(
        [FromRoute] Guid projectId,
        [FromRoute] Guid userId,
        [FromBody] UpdateProjectMemberRoleRequest request,
        [FromServices] IProjectMemberService projectMemberService,
        CancellationToken cancellationToken = default)
    {
        var result = await projectMemberService.UpdateProjectMemberRoleAsync(projectId, userId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == ProjectErrors.ProjectNotFound || result.Error == ProjectErrors.ProjectMemberNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<Error>>> RemoveProjectMemberAsync(
        [FromRoute] Guid projectId,
        [FromRoute] Guid userId,
        [FromServices] IProjectMemberService projectMemberService,
        CancellationToken cancellationToken = default)
    {
        var result = await projectMemberService.RemoveProjectMemberAsync(projectId, userId, cancellationToken);
        if (result.IsFailure)
        {
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok();
    }
}
