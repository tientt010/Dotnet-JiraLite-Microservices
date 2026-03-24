using System;
using Logging.Domain.Entities;

namespace Logging.Application.Interfaces;

public interface ILoggingReadDbContext
{
    IQueryable<ActivityLog> ActivityLogs { get; }
}
