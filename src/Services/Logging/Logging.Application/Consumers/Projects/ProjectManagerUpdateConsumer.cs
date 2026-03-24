using System;
using JiraLite.Shared.Messaging.Events.Projects;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Projects;

public class ProjectManagerUpdateConsumer(IActivityLogRepository repository, ILogger<ProjectManagerUpdateConsumer> logger) : ActivityLogConsumerBase<ProjectManagerUpdatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(ProjectManagerUpdatedEvent message)
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
                TargetType.PROJECT,
                message.ProjectId,
                message.ProjectCode,
                message.ProjectName
            ),
            Changes = [
                new LogChange
                {
                    Field = "Project Manager",
                    Type = ChangeValueType.User,
                    OldValue = message.OldManagerName,
                    OldCode = message.OldManagerCode,
                    OldId = message.OldManagerId,
                    NewValue = message.NewManagerName,
                    NewCode = message.NewManagerCode,
                    NewId = message.NewManagerId
                }
            ]
        };
    }
}
