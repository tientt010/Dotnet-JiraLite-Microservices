using System;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JiraLite.Api.Apis;

public static class ProjectsApi
{
    public static RouteGroupBuilder MapProjectsApi(this RouteGroupBuilder group)
    {
        var projects = group.MapGroup("/projects").WithTags("Projects");

        projects.MapGet("/", GetAllProjectsAsync);
        projects.MapGet("/{projectId:guid}", GetProjectByIdAsync);
        projects.MapPost("/", CreateProjectAsync);
        projects.MapPut("{projectId:guid}", UpdateProjectAsync);
        projects.MapDelete("{projectId:guid}", DeleteProjectAsync);

        projects.MapGet("/{projectId:guid}/members", GetProjectMembersAsync);
        projects.MapPost("/{projectId:guid}/members", AddProjectMemberAsync);
        projects.MapDelete("/{projectId:guid}/members/{memberId:guid}", DeleteProjectMemberAsync);

        projects.MapGet("/my-projects", GetMyProjectsAsync);
        return projects;
    }

    private static async Task GetMyProjectsAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task GetProjectTasksAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task DeleteProjectMemberAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task AddProjectMemberAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task GetProjectMembersAsync(HttpContext context)
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

    private static async Task GetProjectByIdAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task<Ok> GetAllProjectsAsync()
    {
        return TypedResults.Ok();
    }
}
