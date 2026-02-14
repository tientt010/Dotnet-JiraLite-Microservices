using System;
using JiraLite.Authorization.Constants;
using JiraLite.Authorization.Handlers;
using JiraLite.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JiraLite.Authorization.Extensions;

public static class AuthorizationServiceExtensions
{
    public static IHostApplicationBuilder AddJiraLiteAuthorization(this IHostApplicationBuilder builder)
    {

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.RequireAdmin, policy =>
            {
                policy.RequireRole("Admin");
            });

            options.AddPolicy(PolicyNames.ProjectMember, policy =>
            {
                policy.Requirements.Add(new ProjectMemberRequirement());
            });

            options.AddPolicy(PolicyNames.ProjectManager, policy =>
            {
                policy.Requirements.Add(new ProjectManagerRequirement());
            });

            options.AddPolicy(PolicyNames.AdminOrProjectMember, policy =>
            {
                policy.Requirements.Add(new AdminOrProjectMemberRequirement());
            });

            options.AddPolicy(PolicyNames.AdminOrProjectManager, policy =>
            {
                policy.Requirements.Add(new AdminOrProjectManagerRequirement());
            });

            options.AddPolicy(PolicyNames.ProjectManagerOrAssignee, policy =>
            {
                policy.Requirements.Add(new ProjectManagerOrAssigneeRequirement());
            });
        });

        builder.Services.AddScoped<IAuthorizationHandler, ProjectAuthorizationHandler>();

        builder.Services.AddHttpContextAccessor();

        return builder;
    }
}
