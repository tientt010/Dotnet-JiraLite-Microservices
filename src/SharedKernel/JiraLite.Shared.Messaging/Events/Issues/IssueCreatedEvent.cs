namespace JiraLite.Shared.Messaging.Events.Issues;

public record class IssueCreatedEvent
{
    public string IssueId { get; init; } = string.Empty;
    public string IssueCode { get; init; } = string.Empty;
    public string IssueName { get; init; } = string.Empty;

    public string ActorId { get; init; } = string.Empty;
    public string ActorCode { get; init; } = string.Empty;
    public string ActorName { get; init; } = string.Empty;
    public string? ActorAvatarUrl { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

}
