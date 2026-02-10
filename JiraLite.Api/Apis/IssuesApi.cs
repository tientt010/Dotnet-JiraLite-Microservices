using System;

namespace JiraLite.Api.Apis;

public static class IssuesApi
{
    public static RouteGroupBuilder MapIssuesApi(this RouteGroupBuilder group)
    {
        var issues = group.MapGroup("/issues").WithTags("Issues");

        issues.MapGet("/my-issues", GetMyIssuesInProjectsAsync);


        return issues;
    }

    private static async Task GetMyIssuesInProjectsAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
