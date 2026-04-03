using System;

namespace Comment.Application.Interfaces;

public interface ITrackingService
{
    Task<Result> ValidateMembershipAsync(Guid issueId, Guid authorId, CancellationToken ct);
}
