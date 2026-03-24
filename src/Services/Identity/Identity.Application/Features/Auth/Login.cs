using System;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using JiraLite.Shared.Contracts.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Auth;

public static class Login
{
    public record Command(string Email, string Password) : IRequest<Result<TokenResponse>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
    public class Handler(IPasswordHasher passwordHasher, IUserRepository userRepo, IAuthSessionService authSessionService, IUnitOfWork uow) : IRequestHandler<Command, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(Command cmd, CancellationToken ct = default)
        {
            var user = await userRepo.GetUserByEmailAsync(cmd.Email, ct);
            if (user is null || !passwordHasher.VerifyPassword(user, user.PasswordHash, cmd.Password))
                return Result.Failure<TokenResponse>(AuthErrors.InvalidCredentials);

            if (!user.IsActive)
                return Result.Failure<TokenResponse>(UserErrors.UserInActive);

            var tokenResponse = await authSessionService.CreateSessionAsync(user, ct);
            await uow.SaveChangesAsync(ct);

            return Result.Success(tokenResponse);
        }
    }
}
