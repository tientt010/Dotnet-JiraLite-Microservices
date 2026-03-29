namespace Comment.Application.Dtos;

public record class CommentResponse
{
    public Guid Id { get; init; }
    public Guid IssueId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorCode { get; init; } = string.Empty;
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorAvatarUrl { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public int ReplyCount { get; init; }
    public bool HasReplies => ReplyCount > 0;
}
