using System;
using JiraLite.Share.Common;

namespace Identity.Domain.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "The email or password is incorrect.");
    public static readonly Error InvalidAccessToken = new("Auth.InvalidAccessToken", "The access token is invalid");
    public static readonly Error ExpiredAccessToken = new("Auth.ExpiredAccessToken", "The access token has expired");
    public static readonly Error InvalidRefreshToken = new("Auth.InvalidRefreshToken", "The refresh token is invalid");
    public static readonly Error ExpiredRefreshToken = new("Auth.ExpiredRefreshToken", "The refresh token has expired");
}
