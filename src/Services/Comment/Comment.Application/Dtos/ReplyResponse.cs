namespace Comment.Application.Dtos;

public record class ReplyResponse
{
    public required Guid Id { get; init; }
    public required Guid ParentCommentId { get; init; }

    // Author info
    public required Guid AuthorId { get; init; }
    public required string AuthorCode { get; init; }
    public required string AuthorName { get; init; }
    public string? AuthorAvatarUrl { get; init; }

    public required string Content { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
