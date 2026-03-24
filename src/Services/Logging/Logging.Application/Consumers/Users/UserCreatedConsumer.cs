using System;
using JiraLite.Shared.Messaging.Events.Users;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Users;

public class UserCreatedConsumer(IActivityLogRepository repository, ILogger<UserCreatedConsumer> logger) : ActivityLogConsumerBase<UserCreatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(UserCreatedEvent message)
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
                TargetType.USER,
                message.UserId,
                message.UserCode,
                message.UserName
            ),
            Changes = []
        };
    }
}
