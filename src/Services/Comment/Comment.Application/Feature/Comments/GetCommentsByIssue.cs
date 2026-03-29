using System;
using Comment.Application.Dtos;
using Comment.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comment.Application.Feature.Comments;

public static class GetCommentsByIssue
{
    public record Query(Guid IssueId) : IRequest<Result<List<CommentResponse>>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.IssueId).NotEmpty();
        }
    }

    public class Handler(ICommentReadDbContext context) : IRequestHandler<Query, Result<List<CommentResponse>>>
    {
        public async Task<Result<List<CommentResponse>>> Handle(Query request, CancellationToken ct)
        {
            var comments = await context.Comments
                .Where(c => c.IssueId == request.IssueId && c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    IssueId = c.IssueId,
                    ProjectId = c.ProjectId,
                    AuthorId = c.AuthorId,
                    AuthorCode = c.AuthorCode,
                    AuthorName = c.AuthorName,
                    AuthorAvatarUrl = c.AuthorAvatarUrl,
                    Content = c.Content,
                    ReplyCount = c.Replies.Count
                })
                .ToListAsync(ct);
            return Result<List<CommentResponse>>.Success(comments);
        }
    }
}
