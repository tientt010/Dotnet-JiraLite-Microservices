using System.Security.Cryptography;
using System.Text;
using JiraLite.Auth.Infrastructure.Data;
using JiraLite.Auth.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Auth;
using JiraLite.Share.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JiraLite.Auth.Api.Services;

public class AuthService(
    AuthDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IOptions<JwtSettings> jwtSettings) : IAuthService
{
    private readonly AuthDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<Result<string>> RefreshTokenAsync(
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);
        RefreshToken? existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId, cancellationToken);

        if (existingToken is null || existingToken.RevokedAt is not null)
        {
            return Result.Failure<string>(AuthErrors.InvalidRefreshToken);
        }

        if (existingToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result.Failure<string>(AuthErrors.ExpiredRefreshToken);
        }

        existingToken.RevokedAt = DateTime.UtcNow;

        var newTokenValue = GenerateRefreshToken();
        var newTokenHash = HashToken(newTokenValue);

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = newTokenHash,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays)
        };

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(newTokenValue);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (token is not null && token.RevokedAt is null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    public async Task<Result<UserInfoDto>> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {

        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserInfoDto>(AuthErrors.UserNotFound);
        }

        if (!user.IsActive)
        {
            return Result.Failure<UserInfoDto>(AuthErrors.UserInactive);
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Result.Failure<UserInfoDto>(AuthErrors.InvalidPassword);
        }

        if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new UserInfoDto
        (
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            user.IsActive
        ));
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokenValue = GenerateRefreshToken();
        var tokenHash = HashToken(tokenValue);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays)
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tokenValue;
    }
}