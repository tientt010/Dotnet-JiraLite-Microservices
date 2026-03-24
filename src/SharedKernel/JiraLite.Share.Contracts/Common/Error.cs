using System;

namespace JiraLite.Share.Common;

public record Error(string Code, string Description)
{
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    // So sánh chỉ theo ErrorCode vì record equality sẽ fail khi Errors dictionary khác nhau
    public bool IsValidationError => Code == ValidationError.Code;

    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error ServerError = new("System.ServerError", "An internal server error occurred.");
    public static readonly Error InvalidRequest = new("System.InvalidRequest", "The request is invalid.");
    public static readonly Error EmptyUserId = new("System.EmptyUserId", "User ID cannot be empty.");
    public static readonly Error InvalidAccessToken = new("System.InvalidAccessToken", "The access token is invalid.");
    public static readonly Error ValidationError = new("Validation.Failed", "One or more validation errors occurred.");
}
