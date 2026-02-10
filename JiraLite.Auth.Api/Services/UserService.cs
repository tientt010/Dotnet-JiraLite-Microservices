using System;
using JiraLite.Auth.Infrastructure.Data;
using JiraLite.Auth.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Auth;
using Microsoft.EntityFrameworkCore;

namespace JiraLite.Auth.Api.Services;

public class UserService(AuthDbContext dbContext) : IUserService
{
    private readonly AuthDbContext _dbContext = dbContext;
    public async Task<Result<PaginationResponse<UserInfoDto>>> GetAllUsersAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var users = _dbContext.Users.AsNoTracking();
        var response = new PaginationResponse<UserInfoDto>(
            pagination.PageIndex,
            pagination.PageSize,
            await users.LongCountAsync(cancellationToken),
            await users.OrderBy(u => u.FullName)
                .ThenBy(u => u.Id)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(u => new UserInfoDto
                (
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role.ToString(),
                    u.IsActive
                ))
            .ToListAsync(cancellationToken));
        return Result.Success(response);
    }

    public async Task<Result<UserInfoDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var userDto = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => new UserInfoDto
            (
                u.Id,
                u.FullName,
                u.Email,
                u.Role.ToString(),
                u.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);
        if (userDto is null)
        {
            return Result.Failure<UserInfoDto>(AuthErrors.UserNotFound);
        }
        return Result.Success(userDto);
    }

    public async Task<Result<UserInfoDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userDto = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserInfoDto
            (
                u.Id,
                u.FullName,
                u.Email,
                u.Role.ToString(),
                u.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);
        if (userDto is null)
        {
            return Result.Failure<UserInfoDto>(AuthErrors.UserNotFound);
        }
        return Result.Success(userDto);
    }
}
