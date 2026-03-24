using System;
using JiraLite.Shared.Messaging.Events.Projects;
using Logging.Domain.Entities;
using Logging.Domain.Enums;
using Logging.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Logging.Application.Consumers.Projects;

public class ProjectMemberAddedConsumer(IActivityLogRepository repository, ILogger<ProjectMemberAddedConsumer> logger) : ActivityLogConsumerBase<ProjectMemberAddedEvent>(repository, logger)
{
    protected override ActivityLog BuildLog(ProjectMemberAddedEvent message)
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
                    Field = "Member",
                    Type = ChangeValueType.User,
                    OldValue = null,
                    OldCode = null,
                    OldId = null,
                    NewValue = message.MemberName,
                    NewCode = message.MemberCode,
                    NewId = message.MemberId
                }
            ]
        };
    }
}
