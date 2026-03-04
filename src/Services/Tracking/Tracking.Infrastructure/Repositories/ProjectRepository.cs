using System;
using Tracking.Domain.Entities;
using Tracking.Domain.Interfaces;

namespace Tracking.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    public Task<Project?> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
