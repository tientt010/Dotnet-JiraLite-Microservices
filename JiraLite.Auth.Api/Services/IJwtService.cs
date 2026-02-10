using System;
using System.Security.Claims;
using JiraLite.Share.Dtos.Auth;

namespace JiraLite.Auth.Api.Services;

public interface IJwtService
{
    public string GenerateAccessToken(UserInfoDto userInfoDto);
    public ClaimsPrincipal? ValidateToken(string token);
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
