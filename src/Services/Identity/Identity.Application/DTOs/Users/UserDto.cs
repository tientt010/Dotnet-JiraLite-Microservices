using Identity.Domain.Enums;

namespace Identity.Application.DTOs;

public record class UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required SystemRole Role { get; init; }
    public bool IsActive { get; init; } = true;
    public string? AvatarUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}