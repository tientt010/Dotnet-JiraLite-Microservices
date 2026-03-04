using System;
using System.Security.Claims;
using Identity.Domain.Entities;

namespace Identity.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string refreshToken);
}
