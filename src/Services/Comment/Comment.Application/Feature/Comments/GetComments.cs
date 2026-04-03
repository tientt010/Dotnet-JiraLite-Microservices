using System;
using Comment.Application.Dtos;
using Comment.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comment.Application.Feature.Comments;

public static class GetComments
{
    public record Query(Guid? IssueId, Guid? UserId) : IRequest<Result<List<CommentResponse>>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x)
                .Must(x =>
                    (x.IssueId.HasValue && x.IssueId != Guid.Empty) ^
                    (x.UserId.HasValue && x.UserId != Guid.Empty)
                )
                .WithMessage("Either IssueId or UserId must be provided, but not both.");
        }
    }

    public class Handler(ICommentReadDbContext context) : IRequestHandler<Query, Result<List<CommentResponse>>>
    {
        public async Task<Result<List<CommentResponse>>> Handle(Query request, CancellationToken ct)
        {
            var query = context.Comments;

            if (request.IssueId.HasValue && request.IssueId != Guid.Empty)
            {
                query = query.Where(c => c.IssueId == request.IssueId);
            }
            else if (request.UserId.HasValue && request.UserId != Guid.Empty)
            {
                query = query.Where(c => c.AuthorId == request.UserId);
            }

            // 3. Thực thi Query và Select
            var comments = await query
                .OrderBy(c => c.CreatedAt)
                .Where(c => c.ParentCommentId == null)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    IssueId = c.IssueId,
                    AuthorId = c.AuthorId,
                    AuthorCode = c.AuthorCode,
                    AuthorName = c.AuthorName,
                    AuthorAvatarUrl = c.AuthorAvatarUrl,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    ReplyCount = c.Replies.Count
                })
                .ToListAsync(ct);

            return Result<List<CommentResponse>>.Success(comments);
        }
    }
}
