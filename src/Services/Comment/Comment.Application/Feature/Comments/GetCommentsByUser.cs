using System;
using Comment.Application.Dtos;
using Comment.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comment.Application.Feature.Comments;

public static class GetCommentsByUser
{
    public record Query(Guid UserId) : IRequest<Result<List<CommentResponse>>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class Handler(ICommentReadDbContext context) : IRequestHandler<Query, Result<List<CommentResponse>>>
    {

        public async Task<Result<List<CommentResponse>>> Handle(Query request, CancellationToken ct)
        {
            var comments = await context.ActivityLogs
                .Where(c => c.AuthorId == request.UserId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    IssueId = c.IssueId,
                    ProjectId = c.ProjectId,
                    ParentCommentId = c.ParentCommentId,
                    AuthorId = c.AuthorId,
                    AuthorCode = c.AuthorCode,
                    AuthorName = c.AuthorName,
                    AuthorAvatarUrl = c.AuthorAvatarUrl,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(ct);
            return Result<List<CommentResponse>>.Success(comments);
        }
    }
}

