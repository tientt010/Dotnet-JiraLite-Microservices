using Identity.API.Filters;
using System;

namespace Identity.API.Apis;

public static class IdentityApi
{
    public static IEndpointRouteBuilder MapIdentityApi(this IEndpointRouteBuilder builder)
    {
        var vApi = builder.NewVersionedApi("Identity");
        var v1 = vApi.MapGroup("api/v{version:apiVersion}").HasApiVersion(1, 0);
        v1.MapAuthEndpoints();
        v1.MapUserEndpoints();

        // endpoints nội bộ
        var internalGroup = builder.MapGroup("api/internal")
            .AddEndpointFilter<ApiKeyFilter>()
            .WithTags("Internal");
        internalGroup.MapInternalUserEndpoints();

        return builder;
    }
}
