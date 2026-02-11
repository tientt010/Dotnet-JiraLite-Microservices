using System;
using Microsoft.AspNetCore.Authorization;

namespace JiraLite.Authorization.Requirements;

public record ProjectManagerRequirement : IAuthorizationRequirement
{

}
