using System;

namespace JiraLite.Share.Common;

public class ProjectErrors
{
    public static readonly Error ProjectNotFound = new("Project.NotFound", "The specified project was not found.");
    public static readonly Error EmptyProjectName = new("Project.EmptyName", "Project name cannot be empty.");
    public static readonly Error InvalidProjectId = new("Project.InvalidId", "The provided project ID is invalid.");
    public static readonly Error EmptyProjectId = new("Project.EmptyId", "Project ID cannot be empty.");
    public static readonly Error ProjectMemberAlreadyExists = new("Project.MemberAlreadyExists", "The project member already exists.");
    public static readonly Error ProjectMemberNotFound = new("Project.MemberNotFound", "The specified project member was not found.");
    public static readonly Error InvalidProjectRole = new("Project.InvalidRole", "The provided project role is invalid.");

}
