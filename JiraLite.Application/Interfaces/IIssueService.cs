using System;
using JiraLite.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Issues;
using JiraLite.Share.Enums;

namespace JiraLite.Application.Interfaces;

public interface IIssueService
{
    Task<Result> RejectIssueAsync(Guid issueId, Guid changedById, CancellationToken cancellationToken);
    Task<Result<IssueDetailDto>> GetIssueByIdAsync(Guid issueId, CancellationToken cancellationToken);
    Task<Result<PaginationResponse<IssueInfoDto>>> GetIssuesAsync(Guid? projectId, Guid? assigneeId, IssueStatus? status, IssuePriority? priority, string? searchStr, PaginationRequest pagination, CancellationToken cancellationToken);
    Task<Result> UpdateIssueAssigneeAsync(Guid issueId, Guid currentUserId, UpdateIssueAssigneeRequest request, CancellationToken cancellationToken);
    Task<Result<IssueInfoDto>> UpdateIssueAsync(Guid issueId, Guid changedById, UpdateIssueRequest request, CancellationToken cancellationToken);
    Task<Result> UpdateIssuePriorityAsync(Guid issueId, Guid changedById, IssuePriority newPriority, CancellationToken cancellationToken);
    Task<Result> UpdateIssueStatusAsync(Guid issueId, Guid changedById, IssueStatus newStatus, CancellationToken cancellationToken);
}