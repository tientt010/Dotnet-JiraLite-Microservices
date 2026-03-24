namespace JiraLite.Shared.Messaging.Events.Users;

public record class UserProfileChange
{
    public required string Field { get; init; }

    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}
