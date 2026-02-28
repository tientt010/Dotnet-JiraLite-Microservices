using System;
using JiraLite.Share.Dtos.Auth;

namespace JiraLite.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserInfoDto>> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken = default);
}
