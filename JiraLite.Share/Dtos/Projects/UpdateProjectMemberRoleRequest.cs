using System.Text.Json.Serialization;
using JiraLite.Share.Enums;

namespace JiraLite.Share.Dtos.Projects;

public record class UpdateProjectMemberRoleRequest
{
    
    public required ProjectRole Role { get; init; }
}
