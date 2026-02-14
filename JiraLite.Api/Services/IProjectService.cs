using System;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Projects;

namespace JiraLite.Api.Services;

public interface IProjectService
{
    Task<Result<PaginationResponse<ProjectSummaryDto>>> GetProjectsAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<Result<ProjectDetailDto>> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<CreateProjectResponse>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result<UpdateProjectResponse>> UpdateProjectAsync(UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<ProjectInfoDto>>> FetchUserProjectsAsync(Guid userId, PaginationRequest pagination, CancellationToken cancellationToken = default);
}
