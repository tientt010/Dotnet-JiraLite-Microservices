using System;
using JiraLite.Shared.Messaging.Events.Users;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Users;

public class UserLoginSucceededConsumer(IActivityLogRepository repository, ILogger<UserLoginSucceededConsumer> logger) : ActivityLogConsumerBase<UserLoginSucceededEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(UserLoginSucceededEvent message)
    {
        return new ActivityLog
        {
            Timestamp = message.OccurredAt,
            ActionType = ActionType.LOGIN,
            Actor = new LogActor(
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

