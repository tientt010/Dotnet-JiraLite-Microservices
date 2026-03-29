using System;
namespace Comment.Domain.Interfaces;

public interface ICommentRepository
{
    Task AddAsync(Entities.Comment comment);
    Task UpdateAsync(Entities.Comment comment);
    Task DeleteAsync(Guid commentId);
}
