using System;

namespace JiraLite.Share.Common;

public static class IssueErrors
{
    public static readonly Error EmptyIssueTitle = new("Issue.EmptyTitle", "The issue title is empty.");
    public static readonly Error InvalidPriority = new("Issue.InvalidPriority", "The issue priority is invalid.");
    public static readonly Error IssueNotFound = new("Issue.NotFound", "The issue was not found.");
    public static readonly Error PermissionDenied = new("Issue.PermissionDenied", "You do not have permission to perform this action.");
}
