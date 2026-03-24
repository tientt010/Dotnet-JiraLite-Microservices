namespace JiraLite.Shared.Messaging.Events.Issues;

public record class IssueStatusUpdatedEvent
{
    public string IssueId { get; init; } = string.Empty;
    public string IssueCode { get; init; } = string.Empty;
    public string IssueName { get; init; } = string.Empty;

    public string ActorId { get; init; } = string.Empty;
    public string ActorCode { get; init; } = string.Empty;
    public string ActorName { get; init; } = string.Empty;
    public string? ActorAvatarUrl { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public required DateTimeOffset OccurredAt { get; init; }
}
