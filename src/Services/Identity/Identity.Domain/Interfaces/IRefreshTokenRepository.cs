using System;
using Identity.Domain.Entities;

namespace Identity.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken ct);
    Task RevokeAsync(Guid tokenId, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
    void Add(RefreshToken refreshToken);
}
