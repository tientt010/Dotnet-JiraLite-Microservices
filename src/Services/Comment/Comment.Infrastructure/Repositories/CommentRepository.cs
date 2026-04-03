using System;
using Comment.Domain.Interfaces;
using Comment.Infrastructure.Data;

namespace Comment.Infrastructure.Repositories;

public class CommentRepository(CommentDbContext context) : ICommentRepository
{
    private readonly CommentDbContext _context = context;

    public async Task AddAsync(Domain.Entities.Comment comment, CancellationToken ct = default)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Domain.Entities.Comment?> GetByIdAsync(Guid commentId, CancellationToken ct = default)
    {
        return await _context.Comments.FindAsync(commentId, ct);
    }

    public async Task UpdateAsync(Domain.Entities.Comment comment, CancellationToken ct = default)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync(ct);
    }
}
