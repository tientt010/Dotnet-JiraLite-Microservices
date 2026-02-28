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

public class IssueService(JiraLiteDbContext dbContext, ILogService logService, ILogger<IssueService> logger) : IIssueService
{
    private readonly JiraLiteDbContext _dbContext = dbContext;
    private readonly ILogService _logService = logService;
    private readonly ILogger<IssueService> _logger = logger;

    public async Task<Result> RejectIssueAsync(Guid issueId, Guid changedById, CancellationToken cancellationToken)
    {
        var issue = await _dbContext.Issues.FindAsync(issueId, cancellationToken);
        if (issue == null)
        {
            return Result.Failure(IssueErrors.IssueNotFound);
        }

        var currentMemberId = await _dbContext.ProjectMembers
            .Where(m => m.ProjectId == issue.ProjectId && m.UserId == changedById && m.IsActive)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        _logService.TrackIssueChange(issueId, currentMemberId, IssueChangeType.Rejected, oldValue: null, newValue: null);
        issue.Status = IssueStatus.Rejected;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IssueDetailDto>> GetIssueByIdAsync(Guid issueId, CancellationToken cancellationToken)
    {
        var issue = await _dbContext.Issues
            .AsNoTracking()
            .Where(i => i.Id == issueId)
            .Select(i => new IssueDetailDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Status = i.Status.ToString(),
                Priority = i.Priority.ToString(),
                AssigneeId = i.AssignedToId,
                AssigneeTo = i.AssignedTo != null ? i.AssignedTo.FullName : null,
                ProjectId = i.ProjectId,
                ProjectName = i.Project.Name,
                ChangeLogs = i.ChangeLogs.OrderByDescending(cl => cl.ChangedAt).Select(cl => new IssueChangeLogDto
                {
                    Id = cl.Id,
                    ChangedType = cl.ChangeType.ToString(),
                    OldValue = cl.OldValue,
                    NewValue = cl.NewValue,
                    Description = cl.Description,
                    ChangedById = cl.ChangedById,
                    ChangedByName = cl.ChangedBy.FullName,
                    ChangedAt = cl.ChangedAt
                }).ToList(),
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (issue == null)
        {
            return Result.Failure<IssueDetailDto>(IssueErrors.IssueNotFound);
        }

        return Result.Success(issue);
    }

    public async Task<Result<PaginationResponse<IssueInfoDto>>> GetIssuesAsync(Guid? projectId, Guid? assigneeId, IssueStatus? status, IssuePriority? priority, string? searchStr, PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var query = _dbContext.Issues.AsNoTracking();
        if (projectId.HasValue)
            query = query.Where(i => i.ProjectId == projectId.Value);

        if (assigneeId.HasValue)
            query = query.Where(i => i.AssignedToId == assigneeId.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(i => i.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(searchStr))
        {
            var searchPattern = $"%{searchStr.Trim()}%";
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

    public async Task<Result> UpdateIssueAssigneeAsync(
        Guid issueId,
        Guid currentUserId,
        UpdateIssueAssigneeRequest request,
        CancellationToken cancellationToken)
    {
        var issue = await _dbContext.Issues
            .AsTracking()
            .FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);

        if (issue == null)
            return Result.Failure(IssueErrors.IssueNotFound);

        var currentMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == issue.ProjectId
                                    && m.UserId == currentUserId
                                    && m.IsActive, cancellationToken);

        if (currentMember == null)
            return Result.Failure(ProjectErrors.ProjectMemberNotFound);

        var isManager = currentMember.Role == ProjectRole.Manager;
        var isAssignee = issue.AssignedToId == currentMember.Id;

        // Nếu request.AssigneeMemberId == null (unassign) thì chỉ manager hoặc assignee mới có quyền thực hiện
        if (request.AssigneeMemberId == null)
        {
            if (!isManager && !isAssignee)
                return Result.Failure(IssueErrors.PermissionDenied);
        }
        // Nếu issue.AssignedToId == null (assign cho người mới) thì chỉ manager hoặc tự user assign cho mình mới có quyền thực hiện
        else if (issue.AssignedToId == null)
        {
            if (!isManager && request.AssigneeMemberId != currentMember.Id)
                return Result.Failure(IssueErrors.PermissionDenied);
        }
        // Nếu không thuộc 2 trường hợp trên (chuyển assign từ người này sang người khác) thì chỉ manager mới có quyền thực hiện
        else
        {
            if (!isManager)
                return Result.Failure(IssueErrors.PermissionDenied);
        }

        string? newAssigneeName = null;
        if (request.AssigneeMemberId.HasValue)
        {
            var newAssignee = await _dbContext.ProjectMembers
                .FirstOrDefaultAsync(m => m.Id == request.AssigneeMemberId.Value
                                        && m.ProjectId == issue.ProjectId
                                        && m.IsActive, cancellationToken);
            if (newAssignee == null)
                return Result.Failure(ProjectErrors.ProjectMemberNotFound);
            newAssigneeName = newAssignee.FullName;
        }

        var oldAssigneeName = issue.AssignedToId.HasValue
            ? await _dbContext.ProjectMembers
                .Where(m => m.Id == issue.AssignedToId.Value)
                .Select(m => m.FullName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        _logService.TrackIssueChange(issueId,
            currentMember.Id,
            IssueChangeType.AssigneeChanged,
            oldValue: oldAssigneeName,
            newValue: newAssigneeName);

        issue.AssignedToId = request.AssigneeMemberId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IssueInfoDto>> UpdateIssueAsync(Guid issueId, Guid changedById, UpdateIssueRequest request, CancellationToken cancellationToken)
    {
        var issue = await _dbContext.Issues.FindAsync(issueId, cancellationToken);
        if (issue == null)
        {
            return Result.Failure<IssueInfoDto>(IssueErrors.IssueNotFound);
        }

        var currentMemberId = await _dbContext.ProjectMembers
            .Where(m => m.ProjectId == issue.ProjectId && m.UserId == changedById && m.IsActive)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentMemberId == Guid.Empty)
        {
            _logger.LogWarning("Member {UserId} not found in project {ProjectId}",
                changedById, issue.ProjectId);
            return Result.Failure<IssueInfoDto>(ProjectErrors.ProjectMemberNotFound);
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            _logService.TrackIssueChange(issueId, currentMemberId, IssueChangeType.TitleChanged, oldValue: issue.Title, newValue: request.Title.Trim());
            issue.Title = request.Title.Trim();
        }

        if (request.Description != null)
        {
            _logService.TrackIssueChange(issueId, currentMemberId, IssueChangeType.DescriptionChanged, oldValue: issue.Description, newValue: request.Description.Trim());
            issue.Description = request.Description.Trim();
        }

        if (request.Status.HasValue && request.Status.Value != issue.Status)
        {
            _logService.TrackIssueChange(issueId, currentMemberId, IssueChangeType.StatusChanged, oldValue: issue.Status.ToString(), newValue: request.Status.Value.ToString());
            issue.Status = request.Status.Value;
        }

        if (request.Priority.HasValue && request.Priority.Value != issue.Priority)
        {
            _logService.TrackIssueChange(issueId, currentMemberId, IssueChangeType.PriorityChanged, oldValue: issue.Priority.ToString(), newValue: request.Priority.Value.ToString());
            issue.Priority = request.Priority.Value;
        }


        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new IssueInfoDto
        {
            Id = issue.Id,
            Title = issue.Title,
            Description = issue.Description,
            Status = issue.Status.ToString(),
            Priority = issue.Priority.ToString(),
            AssigneeId = issue.AssignedToId,
            AssigneeTo = issue.AssignedTo?.FullName,
            CreatedAt = issue.CreatedAt
        };

        return Result.Success(response);
    }

    public async Task<Result> UpdateIssuePriorityAsync(Guid issueId, Guid changedById, IssuePriority newPriority, CancellationToken cancellationToken)
    {
        var issue = await _dbContext.Issues.FindAsync(issueId, cancellationToken);
        if (issue == null)
        {
            return Result.Failure(IssueErrors.IssueNotFound);
        }

        var currentMemberId = await _dbContext.ProjectMembers
            .Where(m => m.ProjectId == issue.ProjectId && m.UserId == changedById && m.IsActive)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentMemberId == Guid.Empty)
        {
            _logger.LogWarning("Member {UserId} not found in project {ProjectId}",
                changedById, issue.ProjectId);
            return Result.Failure(ProjectErrors.ProjectMemberNotFound);
        }

        var oldPriority = issue.Priority;
        issue.Priority = newPriority;

        _logService.TrackIssueChange(issueId, currentMemberId, IssueChangeType.PriorityChanged, oldValue: oldPriority.ToString(), newValue: newPriority.ToString());
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UpdateIssueStatusAsync(Guid issueId, Guid changedById, IssueStatus newStatus, CancellationToken cancellationToken)
    {
        var issue = await _dbContext.Issues.FindAsync(issueId, cancellationToken);
        if (issue == null)
        {
            return Result.Failure(IssueErrors.IssueNotFound);
        }

        if (newStatus == issue.Status)
        {
            return Result.Success();
        }

        var currentMemberId = await _dbContext.ProjectMembers
            .Where(m => m.ProjectId == issue.ProjectId && m.UserId == changedById && m.IsActive)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentMemberId == Guid.Empty)
        {
            _logger.LogWarning("Member {UserId} not found in project {ProjectId}",
                changedById, issue.ProjectId);
            return Result.Failure(ProjectErrors.ProjectMemberNotFound);
        }

        var oldStatus = issue.Status;
        issue.Status = newStatus;

        _logService.TrackIssueChange(issueId, currentMemberId, IssueChangeType.StatusChanged, oldValue: oldStatus.ToString(), newValue: newStatus.ToString());
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
