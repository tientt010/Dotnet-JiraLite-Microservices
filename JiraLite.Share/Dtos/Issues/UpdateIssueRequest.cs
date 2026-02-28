using JiraLite.Share.Enums;

namespace JiraLite.Share.Dtos.Issues;

public record class UpdateIssueRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public IssueStatus? Status { get; init; }
    public IssuePriority? Priority { get; init; }
}
