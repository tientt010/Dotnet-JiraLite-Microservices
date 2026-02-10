using System;
using JiraLite.Auth.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Auth;

namespace JiraLite.Auth.Api.Services;

public interface IUserService
{
    Task<Result<UserInfoDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<UserInfoDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<UserInfoDto>>> GetAllUsersAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
}
