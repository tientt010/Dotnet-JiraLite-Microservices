namespace Logging.Application.Dtos;

public record class LogTargetDto
{
    public required string Type { get; init; }
    public required string Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
}
