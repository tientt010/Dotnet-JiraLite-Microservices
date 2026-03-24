using System;
using JiraLite.Shared.Messaging.Events.Issues;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Issues;

public class IssueCreatedConsumer(IActivityLogRepository repository, ILogger<IssueCreatedConsumer> logger) : ActivityLogConsumerBase<IssueCreatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(IssueCreatedEvent message)
    {
        return new ActivityLog
        {
            Timestamp = message.OccurredAt,
            ActionType = ActionType.CREATE,
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
