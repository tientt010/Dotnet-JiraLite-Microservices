namespace JiraLite.Share.Dtos.Issues;

public record class IssueInfoDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Status { get; init; }
    public required string Priority { get; init; }
    public required Guid? AssigneeId { get; init; }
    public required string? AssigneeTo { get; init; }
    public required DateTime CreatedAt { get; init; }
}
