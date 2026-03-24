using Logging.Domain.Entities;
using Logging.Domain.Interfaces;
using Logging.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Logging.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly LoggingDbContext _context;
    private readonly ILogger<ActivityLogRepository> _logger;

    public ActivityLogRepository(LoggingDbContext context, ILogger<ActivityLogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(ActivityLog log, CancellationToken ct = default)
    {
        try
        {
            await _context.ActivityLogs.AddAsync(log, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Successfully saved activity log with ID: {LogId} (ActionType: {ActionType}, Target: {TargetType}/{TargetId})",
                log.Id, log.ActionType, log.Target.Type, log.Target.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to save activity log with ID: {LogId}. ActionType: {ActionType}, Target: {TargetType}/{TargetId}",
                log.Id, log.ActionType, log.Target.Type, log.Target.Id);
            throw;
        }
    }
}
