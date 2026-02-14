namespace JiraLite.Share.Dtos.Projects;

public record class CreateProjectRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}
