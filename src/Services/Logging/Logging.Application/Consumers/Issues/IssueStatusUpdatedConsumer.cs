using System;
using JiraLite.Shared.Messaging.Events.Issues;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Issues;

public class IssueStatusUpdatedConsumer(IActivityLogRepository repository, ILogger<IssueStatusUpdatedConsumer> logger) : ActivityLogConsumerBase<IssueStatusUpdatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(IssueStatusUpdatedEvent message)
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
                    Field = "Status",
                    Type = ChangeValueType.Status,
                    OldValue = message.OldStatus,
                    OldCode = null,
                    OldId = null,
                    NewValue = message.NewStatus,
                    NewCode = null,
                    NewId = null
                }
            ]
        };
    }

}