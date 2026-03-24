using System;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Domain.Interfaces;
using JiraLite.Share.Common;
using MediatR;

namespace Identity.Application.Features.Users;

public class GetUsers
{
    public record Query(int PageIndex, int PageSize) : IRequest<Result<PaginationResponse<UserDto>>>;
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(50);
        }
    }

    public class Handler(IUserRepository userRepo) : IRequestHandler<Query, Result<PaginationResponse<UserDto>>>
    {
        public async Task<Result<PaginationResponse<UserDto>>> Handle(Query query, CancellationToken ct = default)
        {
            var results = await userRepo.GetUsersAsync(query.PageIndex, query.PageSize, ct);
            var userDtos = results.Items.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            }).ToList();
            var rsponse = new PaginationResponse<UserDto>(
                query.PageIndex,
                query.PageSize,
                results.TotalCount,
                userDtos
            );
            return Result.Success(rsponse);
        }
    }
}
