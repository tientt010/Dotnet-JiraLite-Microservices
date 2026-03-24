namespace Identity.Application.DTOs.Users;

public record class UpdateProfileRequest
{
    public string? FullName { get; init; }
    public string? AvatarUrl { get; init; }
}
