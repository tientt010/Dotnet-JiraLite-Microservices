using JiraLite.Share.Common;

public static class UserErrors
{
    public static readonly Error UserNotFound = new("User.NotFound", "The specified user does not exist.");
    public static readonly Error InvalidPassword = new("User.InvalidPassword", "The provided password is incorrect.");
    public static readonly Error UserInactive = new("User.Inactive", "The user account is inactive.");
    public static readonly Error EmptyCredentials = new("User.EmptyCredentials", "Email and Password are required.");
    public static readonly Error InvalidEmailOrPassword = new("User.InvalidEmailOrPassword", "The email or password provided is incorrect.");
    public static readonly Error EmptyRefreshToken = new("User.EmptyRefreshToken", "Refresh token is required.");
    public static readonly Error EmptyAccessToken = new("User.EmptyAccessToken", "Access token is required.");
    public static readonly Error InvalidRefreshToken = new("User.InvalidRefreshToken", "The provided refresh token is invalid.");
    public static readonly Error ExpiredRefreshToken = new("User.ExpiredRefreshToken", "The provided refresh token has expired.");
    public static readonly Error InvalidAccessToken = new("User.InvalidAccessToken", "The provided access token is invalid.");
}