using System;
using Identity.Domain.Entities;

namespace Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task AddRefreshToken(RefreshToken refreshToken);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct);
}
