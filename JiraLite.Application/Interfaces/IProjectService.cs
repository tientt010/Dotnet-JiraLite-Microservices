using System;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Projects;

namespace JiraLite.Application.Interfaces;

public interface IProjectService
{
    Task<Result<PaginationResponse<ProjectSummaryDto>>> GetProjectsAsync(
        Guid userId, bool isAdmin, string? memberFilter, string? search,
        PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<Result<ProjectDetailDto>> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<CreateProjectResponse>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result<UpdateProjectResponse>> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
