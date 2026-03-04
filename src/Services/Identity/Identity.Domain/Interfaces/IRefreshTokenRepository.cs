using System;
using Identity.Domain.Entities;

namespace Identity.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken ct);
    Task RevokeAllForUser(Guid userId, CancellationToken ct);
    Task Add(RefreshToken refreshToken);
}
