using System;
using System.Net.Http.Json;
using JiraLite.Application.Interfaces;
using JiraLite.Share.Dtos.Auth;

namespace JiraLite.Application.Services;

public class UserService(HttpClient httpClient) : IUserService
{
    public async Task<Result<UserInfoDto>> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/v1/jiralite/internal/users/{userId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Result.Failure<UserInfoDto>(UserErrors.UserNotFound);

        var user = await response.Content.ReadFromJsonAsync<UserInfoDto>(cancellationToken);
        return Result.Success(user!);
    }
}
