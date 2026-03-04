using System;
using System.ComponentModel.Design;
using System.Security.Claims;
using FluentValidation;
using Identity.Application.DTOs.Auth;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using JiraLite.Shared.Contracts.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Auth;

public class RefreshAccessToken
{
    public record Command(string AccessToken, string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AccessToken).NotEmpty();
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    public class Handler(IJwtProvider jwtProvider, ITokenHasher tokenHasher, IRefreshTokenRepository refreshTokenRepo, IUserRepository userRepo, IUnitOfWork uow, IOptions<JwtSettings> jwtOptions) : IRequestHandler<Command, Result<RefreshTokenResponse>>
    {
        public async Task<Result<RefreshTokenResponse>> Handle(Command cmd, CancellationToken ct = default)
        {
            var principal = jwtProvider.GetPrincipalFromExpiredToken(cmd.AccessToken);
            if (principal == null)
                return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidAccessToken);

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidAccessToken);

            var hashedRefreshToken = tokenHasher.HashToken(cmd.RefreshToken);

            var refreshToken = await refreshTokenRepo.GetByTokenAsync(hashedRefreshToken, ct);

            if (refreshToken == null || refreshToken.UserId != userId)
                return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);

            if (refreshToken.RevokedAt != null)
            {
                await refreshTokenRepo.RevokeAllForUser(userId, ct);
                await uow.SaveChangesAsync(ct);
                return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                return Result.Failure<RefreshTokenResponse>(AuthErrors.ExpiredRefreshToken);

            refreshToken.RevokedAt = DateTime.UtcNow;

            var user = await userRepo.GetUserByIdAsync(userId, ct);
            if (user == null)
                return Result.Failure<RefreshTokenResponse>(UserErrors.UserNotFound);

            if (!user.IsActive)
                return Result.Failure<RefreshTokenResponse>(UserErrors.UserInActive);

            var newAccessToken = jwtProvider.GenerateToken(user);

            var newRefreshTokenString = jwtProvider.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenHasher.HashToken(newRefreshTokenString),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryInDays)
            };

            await refreshTokenRepo.Add(newRefreshTokenEntity);

            await uow.SaveChangesAsync(ct);

            return Result.Success(new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                ExpiresIn = jwtOptions.Value.AccessTokenExpiryInMinutes * 60
            });
        }
    }
}
