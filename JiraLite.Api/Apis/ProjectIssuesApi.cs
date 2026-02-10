using System;

namespace JiraLite.Api.Apis;

public static class ProjectIssuesApi
{
    public static RouteGroupBuilder MapProjectIssuesApi(this RouteGroupBuilder group)
    {
        var issues = group.MapGroup("/projects/{projectId:guid}/issues").WithTags("Project Issues");

        issues.MapGet("/my-issues", GetMyIssuesAsync);
        issues.MapGet("/{IssueId:guid}", GetIssueByIdAsync);

        issues.MapGet("/", GetAllIssuesAsync);
        issues.MapPost("/", CreateIssueAsync);
        issues.MapPut("{id:guid}", UpdateIssueAsync);
        issues.MapDelete("{id:guid}", DeleteIssueAsync);
        issues.MapGet("{id:guid}/status", GetIssueStatusAsync);

        return issues;
    }

    private static async Task GetIssueStatusAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task GetMyIssuesAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task DeleteIssueAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task UpdateIssueAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task CreateIssueAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task GetIssueByIdAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static async Task GetAllIssuesAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
