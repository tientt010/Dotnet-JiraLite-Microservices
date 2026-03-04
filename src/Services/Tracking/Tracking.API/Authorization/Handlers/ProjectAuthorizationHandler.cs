using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tracking.API.Authorization.Requirements;
using Tracking.API.Extensions;
using Tracking.Domain.Enums;
using Tracking.Infrastructure.Data;

namespace Tracking.API.Authorization.Handlers;

public class ProjectAuthorizationHandler(
    TrackingDbContext db,
    IHttpContextAccessor httpContextAccessor) : IAuthorizationHandler
{
    private const string ProjectIdRouteKey = "projectId";
    private const string IssueIdRouteKey = "issueId";

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements
            .Where(r => r is ProjectMemberRequirement
                     || r is ProjectManagerRequirement
                     || r is AdminOrProjectMemberRequirement
                     || r is AdminOrProjectManagerRequirement
                     || r is ProjectManagerOrAssigneeRequirement)
            .ToList();

        if (pendingRequirements.Count == 0) return;

        var user = context.User;
        var isAdmin = user.IsAdmin();

        // Admin tự động pass các policy có "AdminOr..."
        if (isAdmin)
        {
            foreach (var req in pendingRequirements.Where(r =>
                         r is AdminOrProjectMemberRequirement ||
                         r is AdminOrProjectManagerRequirement))
            {
                context.Succeed(req);
            }
        }

        var remaining = context.PendingRequirements
            .Where(r => r is ProjectMemberRequirement
                     || r is ProjectManagerRequirement
                     || r is AdminOrProjectMemberRequirement
                     || r is AdminOrProjectManagerRequirement
                     || r is ProjectManagerOrAssigneeRequirement)
            .ToList();

        if (remaining.Count == 0) return;

        if (!user.TryGetUserId(out var userId)) return;
        if (!TryGetProjectId(out var projectId)) return;

        var membership = await db.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId
                      && pm.UserId == userId
                      && pm.IsActive)
            .Select(pm => new { pm.Role, pm.Id })
            .FirstOrDefaultAsync();

        if (membership is null) return;

        var isManager = membership.Role == ProjectRole.Manager;

        foreach (var req in remaining)
        {
            var authorized = req switch
            {
                ProjectMemberRequirement => true,
                AdminOrProjectMemberRequirement => true,
                ProjectManagerRequirement => isManager,
                AdminOrProjectManagerRequirement => isManager,
                ProjectManagerOrAssigneeRequirement => isManager || IsAssignee(membership.Id),
                _ => false
            };

            if (authorized) context.Succeed(req);
        }
    }

    private bool TryGetProjectId(out Guid projectId)
    {
        projectId = Guid.Empty;
        var value = httpContextAccessor.HttpContext?
            .Request.RouteValues[ProjectIdRouteKey]?.ToString();
        return Guid.TryParse(value, out projectId);
    }

    private bool IsAssignee(Guid projectMemberId)
    {
        var issueIdValue = httpContextAccessor.HttpContext?
            .Request.RouteValues[IssueIdRouteKey]?.ToString();

        if (!Guid.TryParse(issueIdValue, out var issueId)) return false;

        return db.Issues
            .AsNoTracking()
            .Any(i => i.Id == issueId && i.AssignedToId == projectMemberId);
    }
}
