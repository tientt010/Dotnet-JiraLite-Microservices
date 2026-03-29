using System;
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
        Guid ProjectId,
        Guid? ParentCommentId,
        Guid AuthorId,
        string AuthorCode,
        string AuthorName,
        string? AuthorAvatarUrl,
        string Content
    ) : IRequest<Result>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.IssueId).NotEmpty();
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.AuthorId).NotEmpty();
            RuleFor(x => x.AuthorCode).NotEmpty();
            RuleFor(x => x.AuthorName).NotEmpty();
            RuleFor(x => x.Content).NotEmpty().MaximumLength(1000);
        }
    }

    public class Handler(
        ICommentRepository commentRepository,
        ITrackingService trackingService,
        ILogger<Handler> logger) : IRequestHandler<Command, Result>
    {

        public async Task<Result> Handle(Command request, CancellationToken ct)
        {

            var validationResult = await trackingService.ValidateAsync(
                request.ProjectId,
                request.IssueId,
                request.AuthorId,
                ct);

            if (!validationResult.IsSuccess)
            {
                logger.LogWarning("Tracking validation failed: {Error}", validationResult.Error);
                return validationResult;
            }

            var comment = new Domain.Entities.Comment
            {
                Id = Guid.NewGuid(),
                IssueId = request.IssueId,
                ProjectId = request.ProjectId,
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

            return Result.Success();
        }
    }

}
