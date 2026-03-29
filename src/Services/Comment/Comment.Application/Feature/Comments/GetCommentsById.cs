using System;
using Comment.Application.Dtos;
using Comment.Application.Interfaces;
using Comment.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comment.Application.Feature.Comments;

public static class GetCommentsById
{
    public record Query(Guid CommentId) : IRequest<Result<CommentResponse>>;

    public class Handler(ICommentReadDbContext context) : IRequestHandler<Query, Result<CommentResponse>>
    {
        public async Task<Result<CommentResponse>> Handle(Query request, CancellationToken ct)
        {
            var commentDto = await context.Comments
                .Where(c => c.Id == request.CommentId && c.DeletedAt == null)
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
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync(ct);
            if (commentDto == null)
            {
                return Result<CommentResponse>.Failure(CommentErrors.CommentNotFound());
            }

            return Result<CommentResponse>.Success(commentDto);
        }
    }
}
