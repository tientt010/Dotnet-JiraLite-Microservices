using System.Text.Json.Serialization;
using JiraLite.Share.Enums;

namespace JiraLite.Share.Dtos.Projects;

public record class AddProjectMemberRequest
{
    public required Guid UserId { get; init; }

    public required ProjectRole Role { get; init; }
}
