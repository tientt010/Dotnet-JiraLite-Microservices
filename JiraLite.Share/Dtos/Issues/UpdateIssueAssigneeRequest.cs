namespace JiraLite.Share.Dtos.Issues;

public record class UpdateIssueAssigneeRequest
{
    public required Guid? AssigneeMemberId { get; init; }
}
