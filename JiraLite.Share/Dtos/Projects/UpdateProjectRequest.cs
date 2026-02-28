namespace JiraLite.Share.Dtos.Projects;

public record class UpdateProjectRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool IsActive { get; init; }
}