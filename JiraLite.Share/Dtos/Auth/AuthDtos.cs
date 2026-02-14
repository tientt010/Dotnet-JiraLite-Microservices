namespace JiraLite.Share.Dtos.Auth;

// Login
// public record LoginRequest(string Email, string Password);

public record class LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record class LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required UserInfoDto UserInfo { get; init; }
}

// Refresh Token
public record class RefreshTokenRequest
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}

public record class RefreshTokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
}

// User
public record class UserInfoDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public bool IsActive { get; init; } = true;
}