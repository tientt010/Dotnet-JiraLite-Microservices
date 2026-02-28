using System;
using JiraLite.Infrastructure.Entities;
using JiraLite.Share.Enums;

namespace JiraLite.Application.Interfaces;

public interface ILogService
{
    void TrackIssueChange(Guid issueId, Guid changedById, IssueChangeType changeType,
        string? oldValue = null, string? newValue = null, string? description = null);
}
