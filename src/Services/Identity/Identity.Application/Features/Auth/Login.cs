using System;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using JiraLite.Shared.Contracts.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Auth;

public static class Login
{
    public record Command(string Email, string Password) : IRequest<Result<LoginResponse>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
    public class Handler(IUserRepository repo, IPasswordHasher<User> passwordHasher, IJwtProvider jwtProvider, ITokenHasher tokenHasher, IOptions<JwtSettings> jwtOptions, IUnitOfWork uow) : IRequestHandler<Command, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(Command cmd, CancellationToken ct = default)
        {
            var user = await repo.GetUserByEmailAsync(cmd.Email, ct);
            if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, cmd.Password).Equals(PasswordVerificationResult.Failed))
                return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);

            if (!user.IsActive)
                return Result.Failure<LoginResponse>(UserErrors.UserInActive);

            var accessToken = jwtProvider.GenerateToken(user);

            var plainRefreshToken = jwtProvider.GenerateRefreshToken();

            var hashedRefreshToken = tokenHasher.HashToken(plainRefreshToken);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = hashedRefreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryInDays)
            };
            await repo.AddRefreshToken(refreshToken);

            await uow.SaveChangesAsync(ct);
            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Result.Success(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = plainRefreshToken,
                ExpiresIn = jwtOptions.Value.AccessTokenExpiryInMinutes * 60,
                UserInfo = userInfo
            });
        }
    }
}
