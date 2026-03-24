namespace JiraLite.Shared.Messaging.Events.Users;

public record class UserProfileUpdatedEvent
{
    public string UserId { get; init; } = string.Empty;
    public string UserCode { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;

    public string ActorId { get; init; } = string.Empty;
    public string ActorCode { get; init; } = string.Empty;
    public string ActorName { get; init; } = string.Empty;
    public string? ActorAvatarUrl { get; init; }

    public List<UserProfileChange> Changes { get; init; } = [];

    public required DateTimeOffset OccurredAt { get; init; }
}
