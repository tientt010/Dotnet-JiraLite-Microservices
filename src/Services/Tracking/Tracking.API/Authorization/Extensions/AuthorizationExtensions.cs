using Microsoft.AspNetCore.Authorization;
using Tracking.API.Authorization.Constants;
using Tracking.API.Authorization.Handlers;
using Tracking.API.Authorization.Requirements;

namespace Tracking.API.Authorization.Extensions;

public static class AuthorizationExtensions
{
    public static IHostApplicationBuilder AddTrackingAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.RequireAdmin, policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy(PolicyNames.ProjectMember, policy =>
                policy.Requirements.Add(new ProjectMemberRequirement()));

            options.AddPolicy(PolicyNames.ProjectManager, policy =>
                policy.Requirements.Add(new ProjectManagerRequirement()));

            options.AddPolicy(PolicyNames.AdminOrProjectMember, policy =>
                policy.Requirements.Add(new AdminOrProjectMemberRequirement()));

            options.AddPolicy(PolicyNames.AdminOrProjectManager, policy =>
                policy.Requirements.Add(new AdminOrProjectManagerRequirement()));

            options.AddPolicy(PolicyNames.ProjectManagerOrAssignee, policy =>
                policy.Requirements.Add(new ProjectManagerOrAssigneeRequirement()));
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IAuthorizationHandler, ProjectAuthorizationHandler>();

        return builder;
    }
}
