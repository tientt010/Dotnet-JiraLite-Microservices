namespace JiraLite.Share.Dtos.Issues;

public record class IssueChangeLogDto
{
    public required Guid Id { get; init; }
    public required string ChangedType { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public required string? Description { get; init; }
    public required Guid ChangedById { get; init; }
    public required string ChangedByName { get; init; }
    public required DateTime ChangedAt { get; init; }
}
