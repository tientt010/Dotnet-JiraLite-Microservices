namespace Identity.Application.DTOs.Auth;

public record class RefreshTokenRequest
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
