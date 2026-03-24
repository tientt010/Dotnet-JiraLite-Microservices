using System;
using JiraLite.Shared.Messaging.Events.Issues;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Issues;

public class IssueDeletedConsumer(IActivityLogRepository repository, ILogger<IssueDeletedConsumer> logger) : ActivityLogConsumerBase<IssueDeletedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(IssueDeletedEvent message)
    {
        return new ActivityLog
        {
            Timestamp = message.OccurredAt,
            ActionType = ActionType.DELETE,
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
            Changes = []
        };
    }
}
