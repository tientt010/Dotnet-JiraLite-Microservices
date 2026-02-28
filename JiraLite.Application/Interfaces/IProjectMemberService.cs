using System;
using JiraLite.Share.Dtos.Projects;

namespace JiraLite.Application.Interfaces;

public interface IProjectMemberService
{
    Task<Result> AddProjectMemberAsync(Guid projectId, AddProjectMemberRequest request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ProjectMemberDto>>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result> RemoveProjectMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);
    Task<Result> UpdateProjectMemberRoleAsync(Guid projectId, Guid userId, UpdateProjectMemberRoleRequest request, CancellationToken cancellationToken);
}
