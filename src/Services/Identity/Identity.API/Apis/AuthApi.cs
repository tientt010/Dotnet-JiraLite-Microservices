using System;
using Identity.Application.DTOs;
using Identity.Application.DTOs.Auth;
using Identity.Application.Features.Auth;
using Identity.Domain.Errors;
using JiraLite.Share.Common;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Apis;

public static class AuthApi
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth").WithTags("Auth");

        auth.MapPost("/login", HandleLogin)
            .Produces<TokenResponse>(200)
            .Produces<Error>(400)
            .Produces(403);

        auth.MapPost("/register", HandleRegister)
            .Produces(200)
            .Produces<Error>(400);

        auth.MapPost("/refresh-token", HandleRefreshToken)
            .Produces<TokenResponse>(200)
            .Produces<Error>(400)
            .Produces(401)
            .Produces(403);

        auth.MapPost("/revoke-token", HandleRevokeToken)
            .Produces(200)
            .Produces<Error>(400)
            .Produces(401);

        return group;
    }

    private static async Task<Results<Ok<TokenResponse>, UnauthorizedHttpResult, BadRequest<Error>>> HandleRevokeToken(
        [FromBody] RevokeTokenRequest request,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new RevokeToken.Command(request.AccessToken, request.RefreshToken), ct);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<TokenResponse>, BadRequest<Error>, UnauthorizedHttpResult, ForbidHttpResult>> HandleRefreshToken(
        [FromBody] RefreshTokenRequest request,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var cmd = new RefreshAccessToken.Command(request.AccessToken, request.RefreshToken);
        var result = await sender.Send(cmd, ct);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == AuthErrors.InvalidRefreshToken || result.Error == AuthErrors.ExpiredRefreshToken)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == AuthErrors.InvalidAccessToken)
                return TypedResults.Unauthorized();
            if (result.Error == UserErrors.UserInActive)
                return TypedResults.Forbid();
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok, BadRequest<Error>>> HandleRegister(
        [FromBody] RegisterRequest request,
        [FromServices] ISender sender,
        CancellationToken ctr
    )
    {
        var cmd = new Register.Command(request.Email, request.Password, request.FullName);
        var result = await sender.Send(cmd, ctr);
        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == AuthErrors.EmailAlreadyInUse)
            {
                return TypedResults.BadRequest(result.Error);
            }
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<TokenResponse>, ForbidHttpResult, BadRequest<Error>>> HandleLogin(
        [FromBody] LoginRequest request,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var cmd = new Login.Command(request.Email, request.Password);
        var result = await sender.Send(cmd, ct);

        if (result.IsFailure)
        {
            if (result.Error.IsValidationError)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == AuthErrors.InvalidCredentials)
                return TypedResults.BadRequest(result.Error);
            if (result.Error == UserErrors.UserInActive)
                return TypedResults.Forbid();
            return TypedResults.BadRequest(result.Error);
        }
        return TypedResults.Ok(result.Value);
    }
}
