namespace Logging.Domain.Entities;

public record LogActor(
    string Id,
    string Code,
    string Name,
    string? AvatarUrl
);