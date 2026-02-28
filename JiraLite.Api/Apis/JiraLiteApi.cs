using System;

namespace JiraLite.Api.Apis;

public static class JiraLiteApi
{
    public static IEndpointRouteBuilder MapJiraLiteApi(this IEndpointRouteBuilder builder)
    {
        var vApi = builder.NewVersionedApi("JiraLite");
        var v1 = vApi.MapGroup("api/v{version:apiVersion}").HasApiVersion(1, 0);

        v1.MapProjectsApi();
        v1.MapProjectIssuesApi();
        v1.MapProjectMembersApi();
        v1.MapIssuesApi();

        return builder;
    }
}
