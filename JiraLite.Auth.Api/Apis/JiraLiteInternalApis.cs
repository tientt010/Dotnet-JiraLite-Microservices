using System;
using System.Security.Cryptography;
using JiraLite.Auth.Api.Filters;
using JiraLite.Auth.Api.Services;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace JiraLite.Auth.Api.Apis;

public static class JiraLiteInternalApis
{
    public static IEndpointRouteBuilder MapInternalApi(this IEndpointRouteBuilder builder)
    {
        var internalGroup = builder.MapGroup("api/v1/jiralite/internal").AddEndpointFilter<ApiKeyFilter>();

        internalGroup.MapGet("/users/{id:guid}", GetUserInfoByIdAsync);
        internalGroup.MapGet("/users/{email}", GetUserInfoByEmailAsync);
        internalGroup.MapGet("/users", GetAllUsersAsync);

        return builder;
    }

    private static async Task<Results<Ok<PaginationResponse<UserInfoDto>>, BadRequest<string>>> GetAllUsersAsync(
        [AsParameters] PaginationRequest pagination,
        IUserService userService)
    {
        var result = await userService.GetAllUsersAsync(pagination);
        if (result.IsFailure)
        {
            return TypedResults.BadRequest("Có lỗi xảy ra khi lấy danh sách người dùng.");
        }
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<UserInfoDto>, NotFound<Error>>> GetUserInfoByEmailAsync(
        string email,
        IUserService userService)
    {
        var result = await userService.GetUserByEmailAsync(email);
        if (result.IsFailure)
        {
            return TypedResults.NotFound(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<UserInfoDto>, NotFound<Error>>> GetUserInfoByIdAsync(
        Guid id,
        IUserService userService)
    {
        var result = await userService.GetUserByIdAsync(id);
        if (result.IsFailure)
        {
            return TypedResults.NotFound(result.Error);
        }

        return TypedResults.Ok(result.Value);
    }


}
