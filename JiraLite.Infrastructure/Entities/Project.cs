using System;

namespace JiraLite.Infrastructure.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public IEnumerable<ProjectMember> Members { get; set; } = [];
    public IEnumerable<Issue> Issues { get; set; } = [];

}

