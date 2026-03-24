using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class RefreshTokenRepository(IdentityDbContext dbContext) : IRefreshTokenRepository
{
    public void Add(RefreshToken refreshToken)
    {
        dbContext.RefreshTokens.Add(refreshToken);
    }

    public Task<RefreshToken?> GetByTokenAsync(string refreshTokenHash, CancellationToken ct)
    {
        return dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash, ct);
    }

    public Task RevokeAsync(Guid tokenId, CancellationToken ct)
    {
        return dbContext.RefreshTokens
            .Where(rt => rt.Id == tokenId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);
    }

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        return dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);
    }
}
