using System;
using Tracking.Domain.Interfaces;

namespace Tracking.Infrastructure.Data;

public class UnitOfWork(TrackingDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await db.SaveChangesAsync(ct);
    }
}
