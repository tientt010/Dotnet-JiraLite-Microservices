using System;

namespace Identity.Application.DTOs;

public record class LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required UserInfoDto UserInfo { get; init; }
}