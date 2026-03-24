using Logging.Domain.Entities;

namespace Logging.Domain.Interfaces;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken ct = default);
}
