using JiraLite.Share.Enums;

namespace JiraLite.Share.Dtos.Issues;

public record class UpdateIssuePriorityRequest
{
    public required IssuePriority Priority { get; init; }
}
