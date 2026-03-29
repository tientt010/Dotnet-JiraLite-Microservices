using System;

namespace Comment.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorCode { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public List<Comment> Replies { get; set; } = [];
}
