namespace Identity.Application.DTOs.Users;

public record class UpdatePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
