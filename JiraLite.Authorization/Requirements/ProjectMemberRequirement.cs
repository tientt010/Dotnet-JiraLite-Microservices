using System;
using Microsoft.AspNetCore.Authorization;

namespace JiraLite.Authorization.Requirements;

public record ProjectMemberRequirement : IAuthorizationRequirement
{

}
