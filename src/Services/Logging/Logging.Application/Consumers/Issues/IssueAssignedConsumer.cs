using System;
using JiraLite.Shared.Messaging.Events.Issues;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Issues;

public class IssueAssignedConsumer(IActivityLogRepository repository, ILogger<IssueAssignedConsumer> logger) : ActivityLogConsumerBase<IssueAssignedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(IssueAssignedEvent message)
    {
        return new ActivityLog
        {
            Timestamp = message.OccurredAt,
            ActionType = ActionType.ASSIGN,
            Actor = new LogActor
            (
                message.ActorId,
                message.ActorCode,
                message.ActorName,
                message.ActorAvatarUrl
            ),
            Target = new LogTarget
            (
                TargetType.ISSUE,
                message.IssueId,
                message.IssueCode,
                message.IssueName
            ),
            Changes =
            [
                new LogChange
                {
                    Field = "Assignee",
                    Type = ChangeValueType.User,
                    OldValue = message.OldAssigneeName,
                    OldCode = message.OldAssigneeCode,
                    OldId = message.OldAssigneeId,
                    NewValue = message.NewAssigneeName,
                    NewCode = message.NewAssigneeCode,
                    NewId = message.NewAssigneeId
                }
            ]
        };
    }
}