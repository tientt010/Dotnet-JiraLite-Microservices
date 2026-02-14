namespace JiraLite.Share.Dtos.Projects;

public record class ProjectMemberDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
}
