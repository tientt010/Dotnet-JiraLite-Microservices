using System;
using System.Security.Claims;
using JiraLite.Share.Enums;
using Microsoft.AspNetCore.Http;

namespace JiraLite.Authorization.Services;

public class IdentityService(IHttpContextAccessor httpContextAccessor) : IIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private const string ProjectIdRouteKey = "projectId";
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            if (User == null)
            {
                throw new InvalidOperationException("No user context available");
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID claim is missing or invalid");
            }
            return userId;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public string? FullName => User?.FindFirstValue(ClaimTypes.Name);

    public SystemRole SystemRole
    {
        get
        {
            var roleClaim = User?.FindFirstValue(ClaimTypes.Role);
            if (Enum.TryParse<SystemRole>(roleClaim, out var role))
            {
                return role;
            }
            return SystemRole.User;
        }
    }
    public bool IsAdmin => SystemRole == SystemRole.Admin;

    public string? GetClaim(string claimType)
    {
        if (User == null)
        {
            return null;
        }
        var claim = User.FindFirst(claimType);
        return claim?.Value;
    }

    public bool TryGetUserId(out Guid userId)
    {
        if (User == null)
        {
            userId = Guid.Empty;
            return false;
        }
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out userId))
        {
            userId = Guid.Empty;
            return false;
        }
        return true;
    }

    public bool TryGetProjectIdFromRoute(out Guid projectId)
    {
        projectId = Guid.Empty;

        var routeValue = _httpContextAccessor.HttpContext?
            .Request.RouteValues[ProjectIdRouteKey]?.ToString();

        return Guid.TryParse(routeValue, out projectId);
    }
}
