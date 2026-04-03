using System;
namespace Comment.Domain.Interfaces;

public interface ICommentRepository
{
    Task<Entities.Comment?> GetByIdAsync(Guid commentId, CancellationToken ct = default);
    Task AddAsync(Entities.Comment comment, CancellationToken ct = default);
    Task UpdateAsync(Entities.Comment comment, CancellationToken ct = default);
}
