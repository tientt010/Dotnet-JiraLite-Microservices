namespace JiraLite.Shared.Messaging.Events.Issues;

public record class IssueChange
{
    public required string Field { get; init; }

    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}