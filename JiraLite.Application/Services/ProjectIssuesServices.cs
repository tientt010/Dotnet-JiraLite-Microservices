using System;
using JiraLite.Application.Interfaces;
using JiraLite.Infrastructure.Data;
using JiraLite.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Issues;
using JiraLite.Share.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JiraLite.Application.Services;

public class ProjectIssuesServices(JiraLiteDbContext dbContext, ILogger<ProjectIssuesServices> logger) : IProjectIssuesServices
{
    private readonly JiraLiteDbContext _dbContext = dbContext;
    private readonly ILogger<ProjectIssuesServices> _logger = logger;

    public async Task<Result<CreateIssueResponse>> CreateIssueAsync(Guid projectId, CreateIssueRequest request, CancellationToken cancellationToken = default)
    {
        var projectExists = await _dbContext.Projects
            .AnyAsync(p => p.Id == projectId && p.IsActive, cancellationToken);
        if (!projectExists)
            return Result.Failure<CreateIssueResponse>(ProjectErrors.ProjectNotFound);


        var issue = new Issue
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            Status = IssueStatus.ToDo,
            Priority = request.Priority,
        };

        _dbContext.Issues.Add(issue);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created issue {IssueId} in project {ProjectId}", issue.Id, projectId);

        var response = new CreateIssueResponse
        {
            Id = issue.Id,
            Title = issue.Title,
            Description = issue.Description,
            Status = issue.Status.ToString(),
            Priority = issue.Priority.ToString(),
            CreatedAt = issue.CreatedAt
        };

        return Result.Success(response);
    }

    public async Task<Result<PaginationResponse<IssueInfoDto>>> GetProjectIssuesAsync(
        Guid projectId,
        IssueStatus? status,
        Guid? assigneeId,
        string? search,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Issues
            .AsNoTracking()
            .Where(i => i.ProjectId == projectId && i.Status != IssueStatus.Rejected);
        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(i => i.AssignedToId == assigneeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search.Trim()}%";
            query = query.Where(i =>
                EF.Functions.ILike(JiraLiteDbContext.ImmutableUnaccent(i.Title), JiraLiteDbContext.ImmutableUnaccent(searchPattern)) ||
                EF.Functions.ILike(JiraLiteDbContext.ImmutableUnaccent(i.Description ?? string.Empty), JiraLiteDbContext.ImmutableUnaccent(searchPattern)));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var issues = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(i => new IssueInfoDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Status = i.Status.ToString(),
                Priority = i.Priority.ToString(),
                AssigneeId = i.AssignedToId,
                AssigneeTo = i.AssignedTo != null ? i.AssignedTo.FullName : null,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var response = new PaginationResponse<IssueInfoDto>(
            pagination.PageIndex,
            pagination.PageSize,
            totalCount,
            issues);
        return Result.Success(response);
    }


}
