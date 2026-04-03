using System;
using System.Security.Claims;
using Comment.API.Extensions;
using Comment.Application.Dtos;
using Comment.Application.Feature.Comments;
using Comment.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Comment.API.Apis;

public static class CommentsApi
{
    public static IEndpointRouteBuilder MapCommentsApi(this IEndpointRouteBuilder builder)
    {
        var vApi = builder.NewVersionedApi("Comments");
        var v1 = vApi.MapGroup("api/v{version:apiVersion}/comments").HasApiVersion(1, 0);

        var authenticated = v1.RequireAuthorization();

        authenticated.MapGet("", GetComments);
        authenticated.MapGet("/{id:guid}", GetCommentById);
        authenticated.MapGet("/{id:guid}/replies", GetReplies);
        authenticated.MapPost("", CreateComment);
        authenticated.MapPut("/{id:guid}", UpdateComment);
        authenticated.MapDelete("/{id:guid}", DeleteComment);
        return builder;
    }

    private static async Task<IResult> DeleteComment(
        [FromRoute] Guid id,
        ClaimsPrincipal user,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var command = new DeleteComment.Command(
           CommentId: id,
           UserId: user.GetUserId()
       );

        var result = await sender.Send(command, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }

        return Results.Ok();
    }

    private static async Task<IResult> CreateComment(
        [FromBody] CreateCommentRequest request,
        ClaimsPrincipal user,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var command = new CreateComment.Command(
            IssueId: request.IssueId,
            ParentCommentId: request.ParentCommentId,
            AuthorId: user.GetUserId(),
            AuthorCode: user.GetUserCode(),
            AuthorName: user.GetUserFullName(),
            AuthorAvatarUrl: user.GetAvatarUrl(),
            Content: request.Content
        );

        var result = await sender.Send(command, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }

        return Results.Ok();
    }

    private static async Task<IResult> UpdateComment(
        [FromRoute] Guid id,
        [FromBody] UpdateCommentRequest request,
        ClaimsPrincipal user,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var command = new UpdateComment.Command(
            CommentId: id,
            Content: request.Content,
            UserId: user.GetUserId()
        );

        var result = await sender.Send(command, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }

        return Results.Ok();
    }

    private static async Task<IResult> GetCommentById(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var query = new GetCommentsById.Query(id);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetReplies(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var query = new GetReplies.Query(id);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetComments(
        [FromQuery] Guid? issueId,
        [FromQuery] Guid? userId,
        [FromServices] ISender sender,
        CancellationToken ct)
    {

        var query = new GetComments.Query(issueId, userId);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.ValidationErrors?.Any() == true)
            {
                var errorDictionary = result.ValidationErrors
                    .GroupBy(x => x.Code)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray());

                return Results.ValidationProblem(errorDictionary);
            }
        }

        return Results.Ok(result.Value);
    }



}
