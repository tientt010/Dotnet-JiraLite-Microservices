using Microsoft.AspNetCore.Authorization;

namespace Tracking.API.Authorization.Requirements;

public record ProjectManagerOrAssigneeRequirement : IAuthorizationRequirement;
