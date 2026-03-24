using System;
using JiraLite.Shared.Messaging.Events.Projects;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Projects;

public class ProjectDeletedConsumer(IActivityLogRepository repository, ILogger<ProjectDeletedConsumer> logger) : ActivityLogConsumerBase<ProjectDeletedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(ProjectDeletedEvent message)
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
                TargetType.PROJECT,
                message.ProjectId,
                message.ProjectCode,
                message.ProjectName
            ),
            Changes = []
        };
    }
}
