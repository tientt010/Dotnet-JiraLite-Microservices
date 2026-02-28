using System;
using JiraLite.Share.Enums;

namespace JiraLite.Infrastructure.Entities;

public class Issue
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.ToDo;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    public Guid ProjectId { get; set; }
    public Guid? AssignedToId { get; set; }
    public Project Project { get; set; } = null!;
    public ProjectMember? AssignedTo { get; set; }
    public ICollection<IssueChangeLog> ChangeLogs { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}