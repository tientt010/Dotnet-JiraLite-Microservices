namespace JiraLite.Shared.Messaging.Events.Users;

public record class UserCreatedEvent
{
    public string UserId { get; init; } = string.Empty;
    public string UserCode { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;

    public string ActorId { get; init; } = string.Empty;
    public string ActorCode { get; init; } = string.Empty;
    public string ActorName { get; init; } = string.Empty;
    public string? ActorAvatarUrl { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }
}
