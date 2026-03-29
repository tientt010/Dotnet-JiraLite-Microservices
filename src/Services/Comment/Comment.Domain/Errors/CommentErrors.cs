using System;
using JiraLite.Share.Common;

namespace Comment.Domain.Errors;

public static class CommentErrors
{
    public static Error CommentNotFound() => new("Comment.NotFound", $"Not found comment with the given id.");
    public static Error Unauthorized() => new("Comment.Unauthorized", $"You are not authorized to perform this action.");
}
