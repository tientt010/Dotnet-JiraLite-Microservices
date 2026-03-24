using System;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.DTOs.Auth;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using JiraLite.Shared.Contracts.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Auth;

public class RefreshAccessToken
{
    public record Command(string AccessToken, string RefreshToken) : IRequest<Result<TokenResponse>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AccessToken).NotEmpty();
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    public class Handler(IJwtProvider jwtProvider, ITokenHasher tokenHasher, IRefreshTokenRepository refreshTokenRepo, IUserRepository userRepo, IUnitOfWork uow, IAuthSessionService authSessionService) : IRequestHandler<Command, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(Command cmd, CancellationToken ct = default)
        {
            var userId = jwtProvider.GetUserIdFromToken(cmd.AccessToken);
            if (userId is null || userId == Guid.Empty)
                return Result.Failure<TokenResponse>(AuthErrors.InvalidAccessToken);

            var hashedRefreshToken = tokenHasher.HashToken(cmd.RefreshToken);

            var refreshToken = await refreshTokenRepo.GetByTokenAsync(hashedRefreshToken, ct);

            if (refreshToken == null || refreshToken.UserId != userId)
                return Result.Failure<TokenResponse>(AuthErrors.InvalidRefreshToken);

            if (refreshToken.RevokedAt != null)
            {
                // Token đã bị revoke → có thể bị đánh cắp, revoke toàn bộ tokens của user
                await refreshTokenRepo.RevokeAllForUserAsync(userId.Value, ct);
                return Result.Failure<TokenResponse>(AuthErrors.InvalidRefreshToken);
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                return Result.Failure<TokenResponse>(AuthErrors.ExpiredRefreshToken);

            var user = await userRepo.GetUserByIdAsync(userId.Value, ct);
            if (user == null)
                return Result.Failure<TokenResponse>(AuthErrors.InvalidAccessToken);

            if (!user.IsActive)
                return Result.Failure<TokenResponse>(UserErrors.UserInActive);

            await refreshTokenRepo.RevokeAsync(refreshToken.Id, ct);

            var tokenReponse = await authSessionService.CreateSessionAsync(user, ct);

            await uow.SaveChangesAsync(ct);

            return Result.Success(tokenReponse);
        }
    }
}
