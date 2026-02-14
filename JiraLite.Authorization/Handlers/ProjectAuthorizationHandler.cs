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
    IHttpContextAccessor httpContextAccessor) : IAuthorizationHandler
{
    private const string ProjectIdRouteKey = "projectId";
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        // Lấy tất cả requirements liên quan
        var pendingRequirements = context.PendingRequirements
            .Where(r => r is ProjectMemberRequirement
                     || r is ProjectManagerRequirement
                     || r is AdminOrProjectMemberRequirement
                     || r is AdminOrProjectManagerRequirement)
            .ToList();

        if (pendingRequirements.Count == 0) return;

        // Kiểm tra Admin
        var roleClaim = context.User.FindFirstValue(ClaimTypes.Role);
        var isAdmin = roleClaim == "Admin";

        if (isAdmin)
        {
            foreach (var req in pendingRequirements)
            {
                if (req is AdminOrProjectMemberRequirement
                 || req is AdminOrProjectManagerRequirement)
                {
                    context.Succeed(req);
                }
            }
        }

        var remainingRequirements = context.PendingRequirements
            .Where(r => r is ProjectMemberRequirement
                     || r is ProjectManagerRequirement
                     || r is AdminOrProjectMemberRequirement
                     || r is AdminOrProjectManagerRequirement)
            .ToList();

        if (remainingRequirements.Count == 0) return;

        // Lấy userId, projectId
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return;
        if (!TryGetProjectIdFromRoute(out var projectId)) return;

        // Query DB
        var membership = await dbContext.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId && pm.UserId == userId)
            .Select(pm => new { pm.Role })
            .FirstOrDefaultAsync();

        if (membership == null) return;

        // Xử lý từng requirement còn lại
        foreach (var req in remainingRequirements)
        {
            var isAuthorized = req switch
            {
                ProjectMemberRequirement => true,
                AdminOrProjectMemberRequirement => true,
                ProjectManagerRequirement => membership.Role == ProjectRole.Manager,
                AdminOrProjectManagerRequirement => membership.Role == ProjectRole.Manager,
                _ => false
            };

            if (isAuthorized) context.Succeed(req);
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
