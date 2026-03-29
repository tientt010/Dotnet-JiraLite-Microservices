using System;

namespace Comment.Application.Interfaces;

public interface ICommentReadDbContext
{
    IQueryable<Domain.Entities.Comment> ActivityLogs { get; }
}
