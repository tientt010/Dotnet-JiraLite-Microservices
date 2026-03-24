using System;
using JiraLite.Shared.Messaging.Events.Users;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Users;

public class UserProfileUpdatedConsumer(IActivityLogRepository repository, ILogger<UserProfileUpdatedConsumer> logger) : ActivityLogConsumerBase<UserProfileUpdatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(UserProfileUpdatedEvent message)
    {
        return new ActivityLog
        {
            Timestamp = message.OccurredAt,
            ActionType = Logging.Domain.Enums.ActionType.UPDATE,
            Actor = new LogActor
            (
                message.ActorId,
                message.ActorCode,
                message.ActorName,
                message.ActorAvatarUrl
            ),
            Target = new LogTarget
            (
                TargetType.USER,
                message.UserId,
                message.UserCode,
                message.UserName
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
