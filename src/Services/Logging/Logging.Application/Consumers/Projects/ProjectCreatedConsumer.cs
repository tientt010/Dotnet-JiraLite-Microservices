using System;
using JiraLite.Shared.Messaging.Events.Projects;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Projects;

public class ProjectCreatedConsumer(IActivityLogRepository repository, ILogger<ProjectCreatedConsumer> logger) : ActivityLogConsumerBase<ProjectCreatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(ProjectCreatedEvent message)
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
                TargetType.PROJECT,
                message.ProjectId,
                message.ProjectCode,
                message.ProjectName
            ),
            Changes = []
        };
    }
}
