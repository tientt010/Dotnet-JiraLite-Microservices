using System;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using MediatR;

namespace Identity.Application.Features.Users;

public static class GetUserByEmail
{
    public record Query(string Email) : IRequest<Result<UserDto>>;
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class Handler(IUserRepository userRepo) : IRequestHandler<Query, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(Query query, CancellationToken ct = default)
        {
            var user = await userRepo.GetUserByEmailAsync(query.Email, ct);
            if (user is null)
                return Result.Failure<UserDto>(UserErrors.UserNotFound);

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
            return Result.Success(userInfo);
        }
    }

}