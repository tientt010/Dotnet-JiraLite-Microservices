namespace JiraLite.Share.Dtos.Issues;

public record class CreateIssueResponse
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Status { get; init; }
    public required string Priority { get; init; }
    public required DateTime CreatedAt { get; init; }
}
