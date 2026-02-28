using System.Text.Json.Serialization;
using JiraLite.Share.Enums;

namespace JiraLite.Share.Dtos.Issues;

public record CreateIssueRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }


    public required IssuePriority Priority { get; init; }
}
