namespace Identity.Application.DTOs.Auth;

public record class RevokeTokenRequest

{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
