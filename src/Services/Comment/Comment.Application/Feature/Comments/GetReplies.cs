using System;
using Comment.Application.Dtos;
using Comment.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comment.Application.Feature.Comments;

public static class GetReplies
{
    public record Query(Guid CommentId) : IRequest<Result<List<ReplyResponse>>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.CommentId).NotEmpty();
        }
    }

    public class Handler(ICommentReadDbContext context)
        : IRequestHandler<Query, Result<List<ReplyResponse>>>
    {
        public async Task<Result<List<ReplyResponse>>> Handle(
            Query request, CancellationToken ct)
        {
            var replies = await context.Comments
                .Where(c => c.ParentCommentId == request.CommentId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new ReplyResponse
                {
                    Id = c.Id,
                    ParentCommentId = c.ParentCommentId!.Value,
                    AuthorId = c.AuthorId,
                    AuthorCode = c.AuthorCode,
                    AuthorName = c.AuthorName,
                    AuthorAvatarUrl = c.AuthorAvatarUrl,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(ct);

            return Result<List<ReplyResponse>>.Success(replies);
        }
    }

}
