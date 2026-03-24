using System;

namespace Identity.Application.DTOs;

public record class TokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required UserDto UserInfo { get; init; }
}