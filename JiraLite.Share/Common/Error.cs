using System;

namespace JiraLite.Share.Common;

public record Error(string ErrorCode, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error ServerError = new("System.ServerError", "An internal server error occurred.");
    public static readonly Error ValidationError = new("System.ValidationError", "One or more validation errors occurred.");
    public static readonly Error InvalidRequest = new("System.InvalidRequest", "The request is invalid.");
    public static readonly Error EmptyUserId = new("System.EmptyUserId", "User ID cannot be empty.");
}
