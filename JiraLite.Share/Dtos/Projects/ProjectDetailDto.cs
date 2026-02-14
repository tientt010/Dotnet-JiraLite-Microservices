namespace JiraLite.Share.Dtos.Projects;

public record class ProjectDetailDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<ProjectMemberDto> Members { get; init; } = [];
    public IReadOnlyList<IssueInfoDto> Issues { get; init; } = [];
}
