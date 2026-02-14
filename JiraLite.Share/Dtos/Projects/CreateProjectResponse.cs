namespace JiraLite.Share.Dtos.Projects;

public record class CreateProjectResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required bool IsActive { get; init; }
}
