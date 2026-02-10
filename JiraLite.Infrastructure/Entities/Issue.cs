using System;

namespace JiraLite.Infrastructure.Entities;

public class Issue
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.InProgress;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    public Guid ProjectId { get; set; }
    public Guid? AssignedToId { get; set; }
    public Project Project { get; set; } = null!;
    public ProjectMember? AssignedTo { get; set; }
    public ICollection<IssueChangeLog> ChangeLogs { get; set; } = [];
}

public enum IssuePriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum IssueStatus
{
    ToDo = 0,
    InProgress = 1,
    Done = 2
}
