using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JiraLite.Share.Dtos.Auth;
using JiraLite.Share.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JiraLite.Auth.Api.Services;

public class JwtService(IOptions<JwtSettings> jwtSettings) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public string GenerateAccessToken(UserInfoDto userInfoDto)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userInfoDto.Id.ToString()),
            new(ClaimTypes.Email, userInfoDto.Email),
            new(ClaimTypes.Name, userInfoDto.FullName),
            new(ClaimTypes.Role, userInfoDto.Role.ToString()),
            new("IsActive", userInfoDto.IsActive.ToString())
        };
        var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var authKey = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryInMinutes),
            signingCredentials: signingCredentials
        );

        var tokenString = _tokenHandler.WriteToken(tokenDescriptor);
        return tokenString;
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
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = _tokenHandler.ValidateToken(token, tokenValidationParams, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

}
