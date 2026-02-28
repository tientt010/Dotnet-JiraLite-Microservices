using System;
using System.Runtime.InteropServices;
using System.Security.Claims;
using JiraLite.Auth.Api.Services;
using JiraLite.Auth.Infrastructure.Data;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Auth;
using JiraLite.Share.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JiraLite.Auth.Api.Apis;

public static class JiraLiteAuthApis
{
    public static IEndpointRouteBuilder MapAuthApi(this IEndpointRouteBuilder builder)
    {
        var authGroup = builder.MapGroup("api/v1/jiralite");

        authGroup.MapPost("/login", HandleLoginAsync);
        authGroup.MapPost("/refresh-token", HandleRefreshTokenAsync);

        return builder;
    }

    private static async Task<Results<Ok<RefreshTokenResponse>, BadRequest<Error>>> HandleRefreshTokenAsync(
        [FromBody] RefreshTokenRequest refreshTokenRequest,
        IAuthService authService,
        IJwtService jwtService,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenRequest.RefreshToken))
        {
            return TypedResults.BadRequest(UserErrors.EmptyRefreshToken);
        }

        if (string.IsNullOrWhiteSpace(refreshTokenRequest.AccessToken))
        {
            return TypedResults.BadRequest(UserErrors.EmptyAccessToken);
        }
        var principal = jwtService.GetPrincipalFromExpiredToken(refreshTokenRequest.AccessToken);

        if (principal is null)
        {
            return TypedResults.BadRequest(UserErrors.InvalidAccessToken);
        }
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await authService.RefreshTokenAsync(userId, refreshTokenRequest.RefreshToken, cancellationToken);
        if (result.IsFailure)
        {
            return TypedResults.BadRequest(result.Error);
        }

        var userInfoDto = new UserInfoDto
        {
            Id = userId,
            FullName = principal.FindFirstValue(ClaimTypes.Name)!,
            Email = principal.FindFirstValue(ClaimTypes.Email)!,
            Role = principal.FindFirstValue(ClaimTypes.Role)!,
            IsActive = principal.FindFirstValue("IsActive") == "True"
        };
        var newAccessToken = jwtService.GenerateAccessToken(userInfoDto);
        var newRefreshToken = result.Value!;

        return TypedResults.Ok(
            new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 15 * 60
            });
    }

    private static async Task<Results<Ok<LoginResponse>, BadRequest<Error>>> HandleLoginAsync(
        [FromBody] LoginRequest loginRequest,
        IAuthService authService,
        IJwtService jwtService,
        IOptions<JwtSettings> jwtOptions,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.Email) ||
        string.IsNullOrWhiteSpace(loginRequest.Password))
        {
            return TypedResults.BadRequest(UserErrors.EmptyCredentials);
        }

        var emailNormalized = loginRequest.Email.Trim().ToLowerInvariant();
        var result = await authService.ValidateUserAsync(emailNormalized, loginRequest.Password, cancellationToken);

        if (result.IsFailure)
        {
            var error = result.Error;
            if (error == UserErrors.UserNotFound || error == UserErrors.InvalidPassword)
            {
                return TypedResults.BadRequest(UserErrors.InvalidEmailOrPassword);
            }
            else if (error == UserErrors.UserInactive)
            {
                return TypedResults.BadRequest(UserErrors.UserInactive);
            }
            else
            {
                return TypedResults.BadRequest(Error.ServerError);
            }
        }

        var userInfoDto = result.Value!;
        var settings = jwtOptions.Value;

        var refreshToken = await authService.CreateRefreshTokenAsync(userInfoDto.Id, cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = jwtService.GenerateAccessToken(userInfoDto),
            RefreshToken = refreshToken,
            ExpiresIn = settings.AccessTokenExpiryInMinutes * 60,
            UserInfo = userInfoDto
        };

        return TypedResults.Ok(response);

    }
}
