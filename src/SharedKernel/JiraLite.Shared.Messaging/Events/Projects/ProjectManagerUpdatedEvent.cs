namespace JiraLite.Shared.Messaging.Events.Projects;

public record class ProjectManagerUpdatedEvent
{
    public string ProjectId { get; init; } = string.Empty;
    public string ProjectCode { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;

    public string ActorId { get; init; } = string.Empty;
    public string ActorCode { get; init; } = string.Empty;
    public string ActorName { get; init; } = string.Empty;
    public string? ActorAvatarUrl { get; init; }


    public string OldManagerId { get; init; } = string.Empty;
    public string OldManagerCode { get; init; } = string.Empty;
    public string OldManagerName { get; init; } = string.Empty;
    public string NewManagerId { get; init; } = string.Empty;
    public string NewManagerCode { get; init; } = string.Empty;
    public string NewManagerName { get; init; } = string.Empty;

    public required DateTimeOffset OccurredAt { get; init; }
}
