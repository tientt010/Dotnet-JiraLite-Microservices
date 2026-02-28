

using JiraLite.Share.Enums;

namespace JiraLite.Authorization.Services;

public interface IIdentityService
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    SystemRole SystemRole { get; }
    bool IsAdmin { get; }
    bool TryGetUserId(out Guid userId);
    string? GetClaim(string claimType);
    bool TryGetProjectIdFromRoute(out Guid projectId);

}
