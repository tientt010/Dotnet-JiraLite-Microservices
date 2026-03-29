using System;

namespace Comment.Application.Interfaces;

public interface ITrackingService
{
    Task<Result> ValidateAsync(Guid projectId, Guid issueId, Guid authorId, CancellationToken ct);
}
