namespace JiraLite.Shared.Messaging.Events.Users;

public record class UserStatusUpdatedEvent
{
    public string UserId { get; init; } = string.Empty;
    public string UserCode { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;

    public string ActorId { get; init; } = string.Empty;
    public string ActorCode { get; init; } = string.Empty;
    public string ActorName { get; init; } = string.Empty;
    public string? ActorAvatarUrl { get; init; }

    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;

    public required DateTimeOffset OccurredAt { get; init; }
}
