using Logging.Domain.Enums;

namespace Logging.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; set; }
    public required DateTimeOffset Timestamp { get; init; }
    public required ActionType ActionType { get; init; }
    public required LogActor Actor { get; init; }
    public required LogTarget Target { get; init; }
    public ICollection<LogChange> Changes { get; set; } = [];
}

