using Microsoft.AspNetCore.Authorization;

namespace JiraLite.Authorization.Requirements;

public record class AdminOrProjectManagerRequirement : IAuthorizationRequirement
{

}
