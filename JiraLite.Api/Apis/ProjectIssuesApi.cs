using System;
using JiraLite.Authorization.Constants;

namespace JiraLite.Api.Apis;

public static class ProjectIssuesApi
{
    public static RouteGroupBuilder MapProjectIssuesApi(this RouteGroupBuilder group)
    {
        var issues = group.MapGroup("/projects/{projectId:guid}/issues").WithTags("Project Issues");

        issues.MapGet("/my-issues", GetMyIssuesAsync);
        issues.MapGet("/{IssueId:guid}", GetIssueByIdAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectMember);

        issues.MapGet("/", GetAllIssuesAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);
        issues.MapPost("/", CreateIssueAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectManager);
        issues.MapPut("/{IssueId:guid}", UpdateIssueAsync)
            .RequireAuthorization(PolicyNames.ProjectManagerOrAssignee);
        issues.MapDelete("/{IssueId:guid}", DeleteIssueAsync)
            .RequireAuthorization(PolicyNames.ProjectManager);
        issues.MapGet("/{IssueId:guid}/status", GetIssueStatusAsync)
            .RequireAuthorization(PolicyNames.AdminOrProjectMember);
        issues.MapPost("/{IssueId:guid}/assignee", AssignIssueAsync)
            .RequireAuthorization(PolicyNames.ProjectManagerOrAssignee);

        return issues;
    }

    private static async Task AssignIssueAsync(HttpContext context)
    {
        throw new NotImplementedException();
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
