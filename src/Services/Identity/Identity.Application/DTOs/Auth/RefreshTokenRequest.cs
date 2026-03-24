namespace Identity.Application.DTOs.Auth;

public record class RefreshTokenRequest
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
