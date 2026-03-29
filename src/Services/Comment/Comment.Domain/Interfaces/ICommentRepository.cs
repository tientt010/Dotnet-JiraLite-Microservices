using System;
namespace Comment.Domain.Interfaces;

public interface ICommentRepository
{
    Task<Entities.Comment?> GetByIdAsync(Guid commentId);
    Task AddAsync(Entities.Comment comment);
    Task UpdateAsync(Entities.Comment comment);
    // Task DeleteAsync(Guid commentId);
}
