using System;

namespace JiraLite.Share.Common;

public class ProjectErrors
{
    public static readonly Error ProjectNotFound = new("Project_Not_Found", "The specified project was not found.");
    public static readonly Error EmptyProjectName = new("Empty_Project_Name", "Project name cannot be empty.");
}
