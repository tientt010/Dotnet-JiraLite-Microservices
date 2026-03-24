using System;
using JiraLite.Shared.Messaging.Events.Projects;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Projects;

public class ProjectUpdatedConsumer(IActivityLogRepository repository, ILogger<ProjectUpdatedConsumer> logger) : ActivityLogConsumerBase<ProjectUpdatedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(ProjectUpdatedEvent message)
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
            Changes = message.Changes.Select(change => new LogChange
            {
                Field = change.Field,
                Type = ChangeValueType.Text,
                OldValue = change.OldValue,
                OldCode = null,
                OldId = null,
                NewValue = change.NewValue,
                NewCode = null,
                NewId = null
            }).ToList()
        };
    }
}