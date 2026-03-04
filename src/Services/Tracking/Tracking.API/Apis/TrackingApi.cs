using System;
using JiraLite.Api.Apis;
using Tracking.API.Apis;

namespace Tracking.Api.Apis;

public static class TrackingApi
{
    public static IEndpointRouteBuilder MapTrackingApi(this IEndpointRouteBuilder builder)
    {
        var vApi = builder.NewVersionedApi("Tracking");
        var v1 = vApi.MapGroup("api/v{version:apiVersion}").HasApiVersion(1, 0);

        v1.MapProjectsApi();
        v1.MapProjectIssuesApi();
        v1.MapProjectMembersApi();
        v1.MapIssuesApi();

        return builder;
    }
}
