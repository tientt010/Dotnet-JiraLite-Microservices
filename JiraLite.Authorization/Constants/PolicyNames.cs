using System;

namespace JiraLite.Authorization.Constants;

public static class PolicyNames
{
    public const string RequireAdmin = "RequireAdminRole";
    public const string ProjectMember = "ProjectMember";
    public const string ProjectManager = "ProjectManager";
    public const string IssueAssgineeOrManager = "IssueAssgineeOrManager";

}
