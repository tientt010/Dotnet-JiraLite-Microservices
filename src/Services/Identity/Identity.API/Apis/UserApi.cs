using System;
using System.Security.Claims;
using Identity.Application.DTOs;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Users;
using Identity.Domain.Errors;
using JiraLite.Share.Common;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Apis;

public static class UserApi
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        var user = group.MapGroup("/users").WithTags("Users");

        user.MapGet("/{id:guid}", GetUserByIdAsync)
            .RequireAuthorization()
            .Produces<UserDto>(200)
            .Produces<Error>(404)
            .Produces<Error>(400);

        user.MapGet("/{email}", GetUserByEmailAsync)
            .RequireAuthorization()
            .Produces<UserDto>(200)
            .Produces<Error>(404)
            .Produces<Error>(400);

        user.MapGet("/", GetUsersAsync)
            .RequireAuthorization("AdminOnly")
            .Produces<PaginationResponse<UserDto>>(200)
            .Produces<Error>(400);

        user.MapGet("/me/profile", GetCurrentUserProfileAsync)
            .RequireAuthorization()
            .Produces<UserDto>(200)
            .Produces<Error>(404)
            .Produces<Error>(400);

        user.MapPut("/me/profile", UpdateProfileAsync)
            .RequireAuthorization()
            .Produces(200)
            .Produces<Error>(400)
            .Produces(403);

        user.MapPatch("/me/password", UpdatePasswordAsync)
            .RequireAuthorization()
            .Produces(200)
            .Produces<Error>(400)
            .Produces(403);

        user.MapPatch("/{id:guid}/lock", LockUserAsync)
            .RequireAuthorization("AdminOnly")
            .Produces(200)
            .Produces<Error>(400);

        return group;
    }



    public static IEndpointRouteBuilder MapInternalUserEndpoints(this IEndpointRouteBuilder group)
    {
        group.MapGet("/users/{id:guid}", GetUserByIdAsync)
            .Produces<UserDto>(200)
            .Produces<Error>(404)
            .Produces<Error>(400);

        group.MapGet("/users/{email}", GetUserByEmailAsync)
            .Produces<UserDto>(200)
            .Produces<Error>(404)
            .Produces<Error>(400);

        group.MapGet("/users/list", GetUsersAsync)
            .Produces<PaginationResponse<UserDto>>(200)
            .Produces<Error>(400);

        return group;
    }


    private static async Task<Results<Ok<UserDto>, NotFound<Error>, BadRequest<Error>, ForbidHttpResult>> GetCurrentUserProfileAsync(
        HttpContext context,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return TypedResults.Forbid();
        }

        var query = new GetUserInfoById.Query(userId);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == UserErrors.UserNotFound)
                return TypedResults.NotFound(result.Error);
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok, BadRequest<Error>>> LockUserAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var cmd = new LockUser.Command(id);
        var result = await sender.Send(cmd, ct);

        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            return TypedResults.BadRequest(result.Error);
        }

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<TokenResponse>, BadRequest<Error>, ForbidHttpResult>> UpdatePasswordAsync(
        HttpContext context,
        [FromBody] UpdatePasswordRequest request,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return TypedResults.Forbid();
        }

        var cmd = new UpdatePassword.Command(userId, request.CurrentPassword, request.NewPassword);
        var result = await sender.Send(cmd, ct);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            else if (result.Error == UserErrors.UserNotFound)
                return TypedResults.Forbid();
            else if (result.Error == UserErrors.InvalidPassword)
                return TypedResults.BadRequest(result.Error);

            return TypedResults.BadRequest(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok, ForbidHttpResult, BadRequest<Error>>> UpdateProfileAsync(
        HttpContext httpContext,
        [FromBody] UpdateProfileRequest request,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return TypedResults.Forbid();
        }

        var cmd = new UpdateProfile.Command(userId, request.FullName, request.AvatarUrl);
        var result = await sender.Send(cmd, ct);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == UserErrors.UserNotFound)
                return TypedResults.Forbid();
            return TypedResults.BadRequest(result.Error);
        }

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<UserDto>, NotFound<Error>, BadRequest<Error>>> GetUserByIdAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var query = new GetUserInfoById.Query(id);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == UserErrors.UserNotFound)
                return TypedResults.NotFound(result.Error);
            return TypedResults.BadRequest(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<UserDto>, NotFound<Error>, BadRequest<Error>>> GetUserByEmailAsync(
        [FromRoute] string email,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var query = new GetUserByEmail.Query(email);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == UserErrors.UserNotFound)
                return TypedResults.NotFound(result.Error);
            return TypedResults.BadRequest(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<PaginationResponse<UserDto>>, BadRequest<Error>>> GetUsersAsync(
        [AsParameters] PaginationRequest paginationRequest,
        [FromServices] ISender sender,
        CancellationToken ct
    )
    {
        var query = new GetUsers.Query(paginationRequest.PageIndex, paginationRequest.PageSize);
        var result = await sender.Send(query, ct);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }
}
