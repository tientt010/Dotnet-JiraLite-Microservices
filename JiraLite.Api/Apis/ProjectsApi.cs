using System;
using JiraLite.Authorization.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JiraLite.Api.Apis;

public static class ProjectsApi
{
    public static RouteGroupBuilder MapProjectsApi(this RouteGroupBuilder group)
    {
        var projects = group.MapGroup("/projects").WithTags("Projects");

        projects.MapGet("/", GetAllProjectsAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);
        projects.MapGet("/{projectId:guid}", GetProjectByIdAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectMember);
        projects.MapPost("/", CreateProjectAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);

        projects.MapPut("{projectId:guid}", UpdateProjectAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectManager);
        projects.MapDelete("{projectId:guid}", DeleteProjectAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);


        projects.MapGet("/my-projects", FetchUserProjectsAsync);
        return projects;
    }

    private static async Task FetchUserProjectsAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task DeleteProjectAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task UpdateProjectAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task CreateProjectAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task<Results<Ok<Guid>, BadRequest>> GetProjectByIdAsync(Guid projectId)
    {
        return TypedResults.Ok(projectId);
    }

    private static async Task<Ok> GetAllProjectsAsync()
    {
        return TypedResults.Ok();
    }
}
