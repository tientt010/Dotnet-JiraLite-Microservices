namespace JiraLite.Share.Dtos.Projects;

public record class IssueInfoDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Status { get; init; }
    public required string Priority { get; init; }
    public required Guid? AssigneeId { get; init; }

}
