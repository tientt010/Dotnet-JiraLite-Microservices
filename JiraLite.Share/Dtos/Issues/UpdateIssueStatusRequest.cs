using System.Text.Json.Serialization;
using JiraLite.Share.Enums;

namespace JiraLite.Share.Dtos.Issues;

public record UpdateIssueStatusRequest(

    IssueStatus Status
);