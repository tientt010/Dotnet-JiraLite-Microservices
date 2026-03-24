using System;
using JiraLite.Shared.Messaging.Events.Issues;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Issues;

public class IssuePriorityUpdatedConsumer(IActivityLogRepository repository, ILogger<IssuePriorityUpdatedConsumer> logger) : ActivityLogConsumerBase<IssuePriorityUpdatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(IssuePriorityUpdatedEvent message)
    {
        return new ActivityLog
        {
            Timestamp = message.OccurredAt,
            ActionType = ActionType.UPDATE,
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
                    Field = "Priority",
                    Type = ChangeValueType.Priority,
                    OldValue = message.OldPriority,
                    OldCode = null,
                    OldId = null,
                    NewValue = message.NewPriority,
                    NewCode = null,
                    NewId = null
                }
            ]
        };
    }
}