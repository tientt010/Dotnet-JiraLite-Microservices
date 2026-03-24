using System;
using JiraLite.Share.Common;

namespace Identity.Domain.Errors;

public static class UserErrors
{
    public static readonly Error UserNotFound = new("User.UserNotFound", "The user was not found.");
    public static readonly Error UserInActive = new("User.UserInActive", "The user account is inactive.");
    public static readonly Error InvalidPassword = new("User.InvalidPassword", "The current password is incorrect.");
    public static readonly Error UserAlreadyLocked = new("User.UserAlreadyLocked", "The user account is already locked.");
}
