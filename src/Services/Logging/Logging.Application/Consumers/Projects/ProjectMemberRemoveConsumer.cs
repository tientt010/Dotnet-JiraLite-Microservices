using System;
using JiraLite.Shared.Messaging.Events.Projects;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Projects;

public class ProjectMemberRemoveConsumer(IActivityLogRepository repository, ILogger<ProjectMemberRemoveConsumer> logger) : ActivityLogConsumerBase<ProjectMemberRemovedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(ProjectMemberRemovedEvent message)
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
            Changes = [
                new LogChange
                {
                    Field = "Member",
                    Type = ChangeValueType.User,
                    OldValue = message.MemberName,
                    OldCode = message.MemberCode,
                    OldId = message.MemberId,
                    NewValue = null,
                    NewCode = null,
                    NewId = null
                }
            ]
        };
    }
}
