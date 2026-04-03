namespace Comment.Application.Dtos;

public record class CreateCommentRequest(
    Guid IssueId,
    Guid? ParentCommentId,
    string Content
);

// public record class CreateCommentRequest
// {
//     public Guid IssueId { get; init; } = Guid.Empty;
//     public Guid ParentCommentId { get; init; } = Guid.Empty;
//     public string Content { get; init; } = string.Empty;
// }