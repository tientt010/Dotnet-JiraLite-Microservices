using System;
using Comment.Domain.Errors;
using Comment.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Comment.Application.Feature.Comments;

public record class UpdateComment
{
    public record Command(
        Guid CommentId,
        string Content,
        Guid UserId
    ) : IRequest<Result>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CommentId).NotEmpty();
            RuleFor(x => x.Content).NotEmpty().MaximumLength(1000);
        }
    }

    public class Handler(
        ICommentRepository commentRepository,
        ILogger<Handler> logger) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken ct)
        {
            var comment = await commentRepository.GetByIdAsync(request.CommentId);
            if (comment == null)
            {
                logger.LogWarning("Comment with id {CommentId} not found.", request.CommentId);
                return Result.Failure(CommentErrors.CommentNotFound());
            }

            if (comment.AuthorId != request.UserId)
            {
                logger.LogWarning("User {UserId} is not the author of comment {CommentId}.", request.UserId, request.CommentId);
                return Result.Failure(CommentErrors.Unauthorized());
            }

            comment.Content = request.Content;
            await commentRepository.UpdateAsync(comment);

            logger.LogInformation("Comment updated successfully.");
            return Result.Success();
        }
    }
}
