using System;
using Comment.Application.Dtos;
using Comment.Application.Interfaces;
using Comment.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Comment.Application.Feature.Comments;

public static class CreateComment
{
    public record Command(
        Guid IssueId,
        Guid? ParentCommentId,
        Guid AuthorId,
        string AuthorCode,
        string AuthorName,
        string? AuthorAvatarUrl,
        string Content
    ) : IRequest<Result<CommentResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.IssueId).NotEmpty();
            RuleFor(x => x.AuthorId).NotEmpty();
            RuleFor(x => x.AuthorCode).NotEmpty();
            RuleFor(x => x.AuthorName).NotEmpty();
            RuleFor(x => x.Content).NotEmpty().MaximumLength(1000);
        }
    }

    public class Handler(
        ICommentRepository commentRepository,
        ITrackingService trackingService,
        ILogger<Handler> logger) : IRequestHandler<Command, Result<CommentResponse>>
    {

        public async Task<Result<CommentResponse>> Handle(Command request, CancellationToken ct)
        {

            var membershipResult = await trackingService.ValidateMembershipAsync(
                request.IssueId,
                request.AuthorId,
                ct);

            if (!membershipResult.IsSuccess)
            {
                return Result<CommentResponse>.Failure(membershipResult.Error);
            }

            var comment = new Domain.Entities.Comment
            {
                Id = Guid.NewGuid(),
                IssueId = request.IssueId,
                ParentCommentId = request.ParentCommentId,
                AuthorId = request.AuthorId,
                AuthorCode = request.AuthorCode,
                AuthorName = request.AuthorName,
                AuthorAvatarUrl = request.AuthorAvatarUrl,
                Content = request.Content,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await commentRepository.AddAsync(comment);
            logger.LogInformation("Comment created successfully.");

            return Result<CommentResponse>.Success(new CommentResponse
            {
                Id = comment.Id,
                IssueId = comment.IssueId,
                AuthorId = comment.AuthorId,
                AuthorCode = comment.AuthorCode,
                AuthorName = comment.AuthorName,
                AuthorAvatarUrl = comment.AuthorAvatarUrl,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ReplyCount = 0
            });
        }
    }

}
