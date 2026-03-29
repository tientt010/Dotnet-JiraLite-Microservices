namespace Comment.Application.Dtos;

public record class CreateCommentRequest(
    Guid IssueId,
    Guid ProjectId,
    Guid? ParentCommentId,
    string Content
);
