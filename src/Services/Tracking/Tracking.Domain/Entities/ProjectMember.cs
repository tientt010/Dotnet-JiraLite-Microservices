using System;
using Tracking.Domain.Enums;

namespace Tracking.Domain.Entities;

public class ProjectMember
{
    public Guid Id { get; set; }
    public ProjectRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string FullName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }

    public ICollection<Issue> AssignedIssues { get; set; } = [];
    public Project Project { get; set; } = default!;
}

