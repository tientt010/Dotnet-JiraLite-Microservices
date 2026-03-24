namespace Logging.Application.Dtos;

public record class ActivityLogDto
{
    public required string LogId { get; init; }
    public required string ActionType { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required LogActorDto Actor { get; init; }
    public required LogTargetDto Target { get; init; }
    public List<LogChangeDto> Changes { get; init; } = [];
}
