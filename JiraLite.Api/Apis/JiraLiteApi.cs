using System;

namespace JiraLite.Api.Apis;

public static class JiraLiteApi
{
    public static IEndpointRouteBuilder MapJiraLiteApi(this IEndpointRouteBuilder builder)
    {
        var vApi = builder.NewVersionedApi("JiraLite");
        var v1 = vApi.MapGroup("api/v{version:apiVersion}/jiralite").HasApiVersion(1, 0);
        v1.MapProjectsApi();
        v1.MapIssuesApi();
        v1.MapProjectIssuesApi();

        return builder;
    }
}
