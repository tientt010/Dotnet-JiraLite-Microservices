using Microsoft.AspNetCore.Authorization;

namespace Tracking.API.Authorization.Requirements;

public record ProjectManagerRequirement : IAuthorizationRequirement;
