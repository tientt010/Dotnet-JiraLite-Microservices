using System;
using JiraLite.Authorization.Constants;

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
        members.MapDelete("/{memberId:guid}", DeleteProjectMemberAsync)
            .RequireAuthorization(PolicyNames.RequireAdmin);

        return members;
    }

    private static async Task GetProjectMembersAsync(HttpContext context)
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
}
