namespace Logging.Application.Dtos;

public record class LogActorDto
{
    public required string Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? AvatarUrl { get; init; }
}