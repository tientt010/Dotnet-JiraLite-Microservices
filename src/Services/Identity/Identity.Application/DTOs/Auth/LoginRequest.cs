namespace Identity.Application.DTOs;

public record class LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
