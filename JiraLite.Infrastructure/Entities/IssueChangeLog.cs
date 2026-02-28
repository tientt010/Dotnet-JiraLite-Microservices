using System;
using JiraLite.Share.Enums;

namespace JiraLite.Infrastructure.Entities;

public class IssueChangeLog
{
    public Guid Id { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public IssueChangeType ChangeType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }

    public Guid IssueId { get; set; }
    public Guid ChangedById { get; set; }

    public Issue Issue { get; set; } = null!;
    public ProjectMember ChangedBy { get; set; } = null!;
}
