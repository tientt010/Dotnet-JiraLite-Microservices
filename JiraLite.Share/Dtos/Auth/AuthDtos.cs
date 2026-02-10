namespace JiraLite.Share.Dtos.Auth;

// Login
public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserInfoDto User
);

// Refresh Token
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record RefreshTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

// User
public record UserInfoDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive = true
);