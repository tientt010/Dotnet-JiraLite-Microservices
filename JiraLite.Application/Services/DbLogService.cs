using System;
using JiraLite.Application.Interfaces;
using JiraLite.Infrastructure.Data;
using JiraLite.Infrastructure.Entities;
using JiraLite.Share.Enums;

namespace JiraLite.Application.Services;

public class DbLogService(JiraLiteDbContext dbContext) : ILogService
{
    private readonly JiraLiteDbContext _dbContext = dbContext;


    public void TrackIssueChange(Guid issueId, Guid changedById, IssueChangeType changeType, string? oldValue = null, string? newValue = null, string? description = null)
    {
        _dbContext.IssueChangeLogs.Add(new IssueChangeLog
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            ChangedById = changedById,
            ChangeType = changeType,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description
        });
    }
}
