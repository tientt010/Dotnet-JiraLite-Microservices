using System;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using MediatR;

namespace Identity.Application.Features.Users;

public static class UpdatePassword
{
    public record class Command(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result<TokenResponse>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CurrentPassword).NotEmpty().MinimumLength(6);
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(6)
                .NotEqual(x => x.CurrentPassword);
        }
    }

    public class Handler(IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo, IUnitOfWork uow, IPasswordHasher passwordHasher, IAuthSessionService authSessionService) : IRequestHandler<Command, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(Command cmd, CancellationToken ct = default)
        {
            var user = await userRepo.GetUserByIdAsync(cmd.UserId, ct, true);
            if (user is null)
                return Result.Failure<TokenResponse>(UserErrors.UserNotFound);

            if (!passwordHasher.VerifyPassword(user, user.PasswordHash, cmd.CurrentPassword))
                return Result.Failure<TokenResponse>(UserErrors.InvalidPassword);

            await refreshTokenRepo.RevokeAllForUserAsync(user.Id, ct);

            var tokenResponse = await authSessionService.CreateSessionAsync(user, ct);

            user.PasswordHash = passwordHasher.HashPassword(user, cmd.NewPassword);
            await uow.SaveChangesAsync(ct);
            return Result.Success(tokenResponse);
        }
    }
}
