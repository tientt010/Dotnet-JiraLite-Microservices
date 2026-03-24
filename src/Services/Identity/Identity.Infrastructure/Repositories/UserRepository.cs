using System;
using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using Identity.Infrastructure.Data;
using JiraLite.Share.Common;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }

    public Task<bool> CheckEmailExistsAsync(string email, CancellationToken ct, bool trackChanges = false)
    {
        var query = dbContext.Users.AsQueryable();
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        return query.AnyAsync(u => u.Email == email, ct);
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct, bool trackChanges = false)
    {
        var query = dbContext.Users.AsQueryable();
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        return query.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct, bool trackChanges = false)
    {
        var query = dbContext.Users.AsQueryable();
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        return query.FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<PaginationResponse<User>> GetUsersAsync(int pageIndex, int pageSize, CancellationToken ct, bool trackChanges = false)
    {
        var query = dbContext.Users.AsQueryable();
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new PaginationResponse<User>(pageIndex, pageSize, totalCount, items);
    }
}
