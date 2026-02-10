using System;

namespace JiraLite.Share.Common;

public record Error(string ErrorCode, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error ServerError = new("SERVER_ERROR", "An internal server error occurred.");
}
