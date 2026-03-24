using System;

namespace JiraLite.Shared.Messaging.Events.Projects;

public record class ProjectMemberAddedEvent
{
    public string ProjectId { get; init; } = string.Empty;
    public string ProjectCode { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string ActorCode { get; init; } = string.Empty;
    public string ActorName { get; init; } = string.Empty;
    public string? ActorAvatarUrl { get; init; }
    public string MemberId { get; init; } = string.Empty;
    public string MemberCode { get; init; } = string.Empty;
    public string MemberName { get; init; } = string.Empty;

    public required DateTimeOffset OccurredAt { get; init; }
}
