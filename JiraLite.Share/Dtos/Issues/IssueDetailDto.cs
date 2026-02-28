namespace JiraLite.Share.Dtos.Issues;

public record class IssueDetailDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Status { get; init; }
    public required string Priority { get; init; }
    public required Guid? AssigneeId { get; init; }
    public required string? AssigneeTo { get; init; }
    public required Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public IReadOnlyList<IssueChangeLogDto> ChangeLogs { get; init; } = [];
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
