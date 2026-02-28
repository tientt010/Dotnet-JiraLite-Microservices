using System;
using JiraLite.Application.Interfaces;
using JiraLite.Authorization.Constants;
using JiraLite.Authorization.Services;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Projects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Api.Apis;

public static class ProjectsApi
{
    public static RouteGroupBuilder MapProjectsApi(this RouteGroupBuilder group)
    {
        var projects = group.MapGroup("/projects").WithTags("Projects");

        projects.MapGet("/", GetProjectsAsync)
            .RequireAuthorization();

        projects.MapPost("/", CreateProjectAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);

        projects.MapDelete("/{projectId:guid}", DeleteProjectAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);

        projects.MapGet("/{projectId:guid}", GetProjectByIdAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectMember);

        projects.MapPut("/{projectId:guid}", UpdateProjectAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectManager);

        return projects;
    }

    private static async Task<Results<NoContent, NotFound<Error>, BadRequest<Error>>> DeleteProjectAsync(
        [FromRoute] Guid projectId,
        IProjectService projectService)
    {
        var result = await projectService.DeleteProjectAsync(projectId);

        if (result.IsFailure)
        {
            if (result.Error == ProjectErrors.ProjectNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<UpdateProjectResponse>, NotFound<Error>, BadRequest<Error>>> UpdateProjectAsync(
        [FromRoute] Guid projectId,
        [FromBody] UpdateProjectRequest request,
        IProjectService projectService)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return TypedResults.BadRequest(ProjectErrors.EmptyProjectName);
        }

        var result = await projectService.UpdateProjectAsync(projectId, request);

        if (result.IsFailure)
        {
            if (result.Error == ProjectErrors.ProjectNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<CreateProjectResponse>, BadRequest<Error>>> CreateProjectAsync(
        [FromBody] CreateProjectRequest request,
        IProjectService projectService)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return TypedResults.BadRequest(ProjectErrors.EmptyProjectName);
        }

        var result = await projectService.CreateProjectAsync(request);

        if (result.IsFailure)
        {
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<ProjectDetailDto>, NotFound<Error>, BadRequest<Error>>> GetProjectByIdAsync(
        [FromRoute] Guid projectId,
        IProjectService projectService)
    {
        var result = await projectService.GetProjectByIdAsync(projectId);

        if (result.IsFailure)
        {
            if (result.Error == ProjectErrors.ProjectNotFound)
            {
                return TypedResults.NotFound(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<PaginationResponse<ProjectSummaryDto>>, BadRequest<Error>>> GetProjectsAsync(
        [FromQuery] string? member,
        [FromQuery] string? search,
        [AsParameters] PaginationRequest pagination,
        IProjectService projectService,
        IIdentityService identityService)
    {
        if (!identityService.TryGetUserId(out var userId))
        {
            return TypedResults.BadRequest(Error.EmptyUserId);
        }

        var result = await projectService.GetProjectsAsync(
            userId, identityService.IsAdmin, member, search, pagination);

        if (result.IsFailure)
        {
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }
}
