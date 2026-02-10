using JiraLite.Auth.Infrastructure.Entities;
using JiraLite.Share.Dtos.Auth;
using JiraLite.Share.Settings;

namespace JiraLite.Auth.Api.Services;

public interface IAuthService
{
    Task<Result<UserInfoDto>> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Result<string>> RefreshTokenAsync(
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    string GenerateRefreshToken();
}
