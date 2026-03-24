using System;
using Identity.Domain.Entities;
using JiraLite.Share.Common;

namespace Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct, bool trackChanges = false);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct, bool trackChanges = false);
    void Add(User user);
    Task<bool> CheckEmailExistsAsync(string email, CancellationToken ct, bool trackChanges = false);
    Task<PaginationResponse<User>> GetUsersAsync(int pageIndex, int pageSize, CancellationToken ct, bool trackChanges = false);
}
