using System;
using System.Security.Claims;
using JiraLite.Authorization.Requirements;
using JiraLite.Infrastructure.Data;
using JiraLite.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JiraLite.Authorization.Handlers;

public class ProjectAuthorizationHandler(
    JiraLiteDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ProjectAuthorizationHandler> logger) : IAuthorizationHandler
{
    private const string ProjectIdRouteKey = "projectId";
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements
            .Where(r => r is ProjectMemberRequirement || r is ProjectManagerRequirement)
            .ToList();

        if (pendingRequirements.Count == 0)
        {
            return;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            logger.LogWarning("Invalid or missing user ID claim.");
            return;
        }

        if (!TryGetProjectIdFromRoute(out var projectId))
        {
            logger.LogWarning("Project ID not found in route.");
            return;
        }

        var membership = await dbContext.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.IsActive)
            .Select(pm => new { pm.Role })
            .FirstOrDefaultAsync();

        if (membership == null)
        {
            logger.LogInformation("User {UserId} is not a member of project {ProjectId}.", userId, projectId);
            return;
        }


        foreach (var requirement in pendingRequirements)
        {
            var isAuthorized = requirement switch
            {
                ProjectMemberRequirement => true,
                ProjectManagerRequirement => membership?.Role == ProjectRole.Manager,
                _ => false
            };


            if (isAuthorized)
            {
                context.Succeed(requirement);
            }
        }
    }
    private bool TryGetProjectIdFromRoute(out Guid projectId)
    {
        projectId = Guid.Empty;

        var routeValue = httpContextAccessor.HttpContext?
            .Request.RouteValues[ProjectIdRouteKey]?.ToString();

        return Guid.TryParse(routeValue, out projectId);
    }
}
