namespace JiraLite.Shared.Messaging.Events.Projects;

public record class ProjectChange
{
    public required string Field { get; init; }
    
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}
