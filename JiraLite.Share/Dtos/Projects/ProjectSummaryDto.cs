using System;

namespace JiraLite.Share.Dtos.Projects;

public record class ProjectSummaryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required Guid ManagerId { get; init; }
    public required string ManagerName { get; init; }
    public required int MemberCount { get; init; }
    public required int IssueTodoCount { get; init; }
    public required int IssueInProgressCount { get; init; }
    public required int IssueDoneCount { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
