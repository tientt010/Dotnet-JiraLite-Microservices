using System;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using JiraLite.Shared.Contracts.Settings;
using Microsoft.Extensions.Options;

namespace Identity.Application.Services;

public class AuthSessionService(IJwtProvider jwtProvider, ITokenHasher tokenHasher, IRefreshTokenRepository refreshTokenRepo, IOptions<JwtSettings> jwtOptions) : IAuthSessionService
{
    public async Task<TokenResponse> CreateSessionAsync(User user, CancellationToken ct = default)
    {
        var tokenPair = jwtProvider.GenerateTokenPair(user);

        var hashedRefreshToken = tokenHasher.HashToken(tokenPair.RefreshToken);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = hashedRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryInDays)
        };
        refreshTokenRepo.Add(refreshToken);

        var userInfo = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return new TokenResponse
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            ExpiresIn = tokenPair.ExpiresIn,
            UserInfo = userInfo
        };
    }
}
