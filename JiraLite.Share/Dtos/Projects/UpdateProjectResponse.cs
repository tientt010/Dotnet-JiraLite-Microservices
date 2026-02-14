namespace JiraLite.Share.Dtos.Projects;

public record class UpdateProjectResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool IsActive { get; init; }
}
