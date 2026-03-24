using System;
using System.Security.Claims;
using Identity.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
    Guid? GetUserIdFromToken(string token);
    TokenResult GenerateTokenPair(User user);
}

public record TokenResult
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
}