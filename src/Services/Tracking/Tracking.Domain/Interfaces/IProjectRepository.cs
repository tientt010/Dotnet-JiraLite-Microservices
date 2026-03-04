using System;
using Tracking.Domain.Entities;

namespace Tracking.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken);
}
