using Identity.Domain.Interfaces;

namespace Identity.Infrastructure.Data;

public class UnitOfWork(IdentityDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
