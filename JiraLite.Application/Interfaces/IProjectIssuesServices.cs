using System;
using JiraLite.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Issues;
using JiraLite.Share.Enums;

namespace JiraLite.Application.Interfaces;

public interface IProjectIssuesServices
{
    Task<Result<PaginationResponse<IssueInfoDto>>> GetProjectIssuesAsync(Guid projectId, IssueStatus? status, Guid? assigneeId, string? search, PaginationRequest pagination, CancellationToken cancellationToken = default);

    Task<Result<CreateIssueResponse>> CreateIssueAsync(Guid projectId, CreateIssueRequest request, CancellationToken cancellationToken = default);
}
