using System;
using Comment.Application.Dtos;
using Comment.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comment.Application.Feature.Comments;

public static class GetCommentsByIssue
{
    public record Query(Guid IssueId) : IRequest<List<CommentResponse>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.IssueId).NotEmpty();
        }
    }

    public class Handler(ICommentReadDbContext context) : IRequestHandler<Query, List<CommentResponse>>
    {
        public async Task<List<CommentResponse>> Handle(Query request, CancellationToken ct)
        {
            var comments = await context.ActivityLogs
                .Where(c => c.IssueId == request.IssueId && c.DeletedAt == null)
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

            var commentDict = comments.ToDictionary(c => c.Id);
            var rootComments = new List<CommentResponse>();
            foreach (var comment in comments)
            {
                if (comment.ParentCommentId.HasValue && commentDict.TryGetValue(comment.ParentCommentId.Value, out var parent))
                {
                    // Nếu có cha và cha tồn tại trong list: Add vào con của cha
                    parent.Replies.Add(comment);
                }
                else
                {
                    // Nếu không có cha (hoặc cha bị xóa/không tìm thấy): Đưa ra ngoài cùng
                    rootComments.Add(comment);
                }
            }
            return rootComments;

        }
    }
}
