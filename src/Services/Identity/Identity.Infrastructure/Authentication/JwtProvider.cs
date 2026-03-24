using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using JiraLite.Shared.Contracts.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Authentication;

public class JwtProvider(IOptions<JwtSettings> jwtOptions) : IJwtProvider
{
    private readonly JwtSecurityTokenHandler tokenHandler = new();
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        // Generate JWT token
        var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Value.Secret);
        var authKey = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpiryInMinutes),
            signingCredentials: signingCredentials
        );

        var tokenString = tokenHandler.WriteToken(tokenDescriptor);
        return tokenString;
    }

    public TokenResult GenerateTokenPair(User user)
    {
        var accessToken = GenerateToken(user);
        var refreshToken = GenerateRefreshToken();
        return new TokenResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = jwtOptions.Value.AccessTokenExpiryInMinutes * 60
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        return ValidateTokenInternal(token, validateLifetime: false);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        return ValidateTokenInternal(token, validateLifetime: true);
    }

    private ClaimsPrincipal? ValidateTokenInternal(string token, bool validateLifetime)
    {
        var tokenValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Value.Issuer,
            ValidAudience = jwtOptions.Value.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret)),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParams, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        var principal = ValidateTokenInternal(token, true);
        if (principal == null) return null;

        var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) return null;

        return userId;
    }
}
