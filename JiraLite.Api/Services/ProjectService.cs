using System;
using JiraLite.Infrastructure.Data;
using JiraLite.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Projects;
using Microsoft.EntityFrameworkCore;

namespace JiraLite.Api.Services;

public class ProjectService(JiraLiteDbContext dbContext, ILogger<ProjectService> logger) : IProjectService
{
    private readonly JiraLiteDbContext _dbContext = dbContext;
    private readonly ILogger<ProjectService> _logger = logger;

    public async Task<Result<CreateProjectResponse>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<CreateProjectResponse>(ProjectErrors.EmptyProjectName);
        }
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created new project {ProjectName}", project.Name);

        var response = new CreateProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt
        };

        return Result.Success(response);
    }

    public async Task<Result> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects.FindAsync(projectId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.ProjectNotFound);
        }

        project.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted project with ID {ProjectId}", projectId);
        return Result.Success();
    }

    public async Task<Result<PaginationResponse<ProjectInfoDto>>> FetchUserProjectsAsync(Guid userId, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var projectMembers = _dbContext.ProjectMembers.AsNoTracking().Where(pm => pm.UserId == userId && pm.IsActive);
        var response = new PaginationResponse<ProjectInfoDto>(
            pagination.PageIndex,
            pagination.PageSize,
            await projectMembers.LongCountAsync(cancellationToken),
            await projectMembers
                .OrderByDescending(pm => pm.JoinedAt)
                .ThenBy(pm => pm.Id)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
            .Select(pm => new ProjectInfoDto
            {
                Id = pm.Project.Id,
                Name = pm.Project.Name,
                Description = pm.Project.Description,
                CreatedAt = pm.Project.CreatedAt,
                IsActive = pm.Project.IsActive
            })
            .ToListAsync(cancellationToken));

        return Result.Success(response);
    }

    public async Task<Result<ProjectDetailDto>> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var response = await _dbContext.Projects
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.Id == projectId)
            .Select(p => new ProjectDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                Members = p.Members.Select(m => new ProjectMemberDto
                {
                    Id = m.UserId,
                    FullName = m.FullName,
                    Email = m.Email,
                    Role = m.Role.ToString(),
                }).ToList(),
                Issues = p.Issues.Select(i => new IssueInfoDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    Status = i.Status.ToString(),
                    Priority = i.Priority.ToString(),
                    AssigneeId = i.AssignedToId
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
        {
            return Result.Failure<ProjectDetailDto>(ProjectErrors.ProjectNotFound);
        }

        return Result.Success(response);
    }

    public async Task<Result<PaginationResponse<ProjectSummaryDto>>> GetProjectsAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var projects = _dbContext.Projects.AsNoTracking().Where(p => p.IsActive);
        var totalCount = await projects.LongCountAsync(cancellationToken);
        var response = new PaginationResponse<ProjectSummaryDto>(
            pagination.PageIndex,
            pagination.PageSize,
            totalCount,
            await projects.OrderByDescending(p => p.CreatedAt)
                .ThenBy(p => p.Id)
                .AsSplitQuery()
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(p => new ProjectSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    MemberCount = p.Members.Count(m => m.IsActive),
                    ManagerId = p.Members
                        .Where(m => m.Role == ProjectRole.Manager && m.IsActive)
                        .Select(m => m.UserId)
                        .FirstOrDefault(),

                    ManagerName = p.Members
                        .Where(m => m.Role == ProjectRole.Manager && m.IsActive)
                        .Select(m => m.FullName)
                        .FirstOrDefault() ?? "Unknown",
                    IssueInProgressCount = p.Issues.Count(i => i.Status == IssueStatus.InProgress),
                    IssueTodoCount = p.Issues.Count(i => i.Status == IssueStatus.ToDo),
                    IssueDoneCount = p.Issues.Count(i => i.Status == IssueStatus.Done)
                }).ToListAsync(cancellationToken));

        return Result.Success(response);
    }

    public async Task<Result<UpdateProjectResponse>> UpdateProjectAsync(UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<UpdateProjectResponse>(ProjectErrors.EmptyProjectName);
        }
        var project = await _dbContext.Projects.FindAsync([request.Id], cancellationToken);
        if (project == null)
        {
            return Result.Failure<UpdateProjectResponse>(ProjectErrors.ProjectNotFound);
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.IsActive = request.IsActive;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated project with ID {ProjectId}", request.Id);

        var response = new UpdateProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            IsActive = project.IsActive,
        };
        return Result.Success(response);
    }
}
