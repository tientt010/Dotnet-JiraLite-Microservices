using System;
using JiraLite.Application.Interfaces;
using JiraLite.Infrastructure.Data;
using JiraLite.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Issues;
using JiraLite.Share.Dtos.Projects;
using JiraLite.Share.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JiraLite.Application.Services;

public class ProjectService(
    JiraLiteDbContext dbContext,
    ILogger<ProjectService> logger) : IProjectService
{
    private readonly JiraLiteDbContext _dbContext = dbContext;
    private readonly ILogger<ProjectService> _logger = logger;

    public async Task<Result<CreateProjectResponse>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
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

    public async Task<Result<PaginationResponse<ProjectSummaryDto>>> GetProjectsAsync(
        Guid userId, bool isAdmin, string? memberFilter, string? search,
        PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var shouldScopeToUser = !isAdmin || string.Equals(memberFilter, "me", StringComparison.OrdinalIgnoreCase);

        IQueryable<Project> query;

        if (shouldScopeToUser)
        {
            query = _dbContext.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .Select(pm => pm.Project)
                .Where(p => p.IsActive);
        }
        else
        {
            query = _dbContext.Projects.AsNoTracking().Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search.Trim()}%";

            query = query.Where(p =>
                EF.Functions.ILike(JiraLiteDbContext.ImmutableUnaccent(p.Name), JiraLiteDbContext.ImmutableUnaccent(searchPattern)) ||
                EF.Functions.ILike(JiraLiteDbContext.ImmutableUnaccent(p.Description ?? string.Empty), JiraLiteDbContext.ImmutableUnaccent(searchPattern)));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
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
            })
            .ToListAsync(cancellationToken);

        var response = new PaginationResponse<ProjectSummaryDto>(
            pagination.PageIndex, pagination.PageSize, totalCount, items);

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
                    AssigneeId = i.AssignedToId,
                    AssigneeTo = i.AssignedTo != null ? i.AssignedTo.FullName : null,
                    CreatedAt = i.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
        {
            return Result.Failure<ProjectDetailDto>(ProjectErrors.ProjectNotFound);
        }

        return Result.Success(response);
    }

    public async Task<Result<UpdateProjectResponse>> UpdateProjectAsync(
        Guid projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<UpdateProjectResponse>(ProjectErrors.EmptyProjectName);
        }
        var project = await _dbContext.Projects.FindAsync([projectId], cancellationToken);
        if (project == null)
        {
            return Result.Failure<UpdateProjectResponse>(ProjectErrors.ProjectNotFound);
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.IsActive = request.IsActive;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated project with ID {ProjectId}", projectId);

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
