using System;
using JiraLite.Shared.Messaging.Events.Issues;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Issues;

public class IssueUpdatedConsumer(IActivityLogRepository repository, ILogger<IssueUpdatedConsumer> logger) : ActivityLogConsumerBase<IssueUpdatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(IssueUpdatedEvent message)
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

            Changes = message.Changes.Select(c => new LogChange
            {
                Field = c.Field,
                Type = ChangeValueType.Text,
                OldValue = c.OldValue,
                OldCode = null,
                OldId = null,
                NewValue = c.NewValue,
                NewCode = null,
                NewId = null
            }).ToList()
        };
    }

}
