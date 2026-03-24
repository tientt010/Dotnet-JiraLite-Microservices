using System;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using MediatR;

namespace Identity.Application.Features.Auth;

public class RevokeToken
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

    public class Handler(IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo, ITokenHasher tokenHasher, IJwtProvider jwtProvider, IAuthSessionService authSessionService, IUnitOfWork uow) : IRequestHandler<Command, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(Command cmd, CancellationToken ct = default)
        {
            var userId = jwtProvider.GetUserIdFromToken(cmd.AccessToken);
            if (userId is null || userId == Guid.Empty)
                return Result.Failure<TokenResponse>(AuthErrors.InvalidRefreshToken);
            var user = await userRepo.GetUserByIdAsync(userId.Value, ct);
            if (user == null)
                return Result.Failure<TokenResponse>(AuthErrors.InvalidRefreshToken);

            var hashedToken = tokenHasher.HashToken(cmd.RefreshToken);

            var token = await refreshTokenRepo.GetByTokenAsync(hashedToken, ct);
            if (token == null || token.UserId != userId.Value || token.RevokedAt != null)
                return Result.Failure<TokenResponse>(AuthErrors.InvalidRefreshToken);

            await refreshTokenRepo.RevokeAsync(token.Id, ct);

            var tokenReponse = await authSessionService.CreateSessionAsync(user, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Success(tokenReponse);
        }
    }
}
