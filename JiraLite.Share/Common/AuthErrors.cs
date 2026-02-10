using JiraLite.Share.Common;

public static class AuthErrors
{
    public static readonly Error UserNotFound = new("USER_NOT_FOUND", "The specified user does not exist.");
    public static readonly Error InvalidPassword = new("INVALID_PASSWORD", "The provided password is incorrect.");
    public static readonly Error UserInactive = new("USER_INACTIVE", "The user account is inactive.");
    public static readonly Error EmptyCredentials = new("EMPTY_CREDENTIALS", "Email and Password are required.");
    public static readonly Error InvalidEmailOrPassword = new("INVALID_EMAIL_OR_PASSWORD", "The email or password provided is incorrect.");
    public static readonly Error EmptyRefreshToken = new("EMPTY_REFRESH_TOKEN", "Refresh token is required.");
    public static readonly Error EmptyAccessToken = new("EMPTY_ACCESS_TOKEN", "Access token is required.");
    public static readonly Error InvalidRefreshToken = new("INVALID_REFRESH_TOKEN", "The provided refresh token is invalid.");
    public static readonly Error ExpiredRefreshToken = new("EXPIRED_REFRESH_TOKEN", "The provided refresh token has expired.");
    public static readonly Error InvalidAccessToken = new("INVALID_ACCESS_TOKEN", "The provided access token is invalid.");
}