using System;
using System.Net;
using Comment.Application.Interfaces;
using Comment.Domain.Errors;
using JiraLite.Share.Common;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services;

public class TrackingServiceClient(HttpClient httpClient, ILogger<TrackingServiceClient> logger) : ITrackingService
{
    public async Task<Result> ValidateMembershipAsync(Guid issueId, Guid authorId, CancellationToken ct)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/v1/issues/{issueId}/validate-member?userId={authorId}", ct);

            if (response.StatusCode == HttpStatusCode.OK) return Result.Success();
            else if (response.StatusCode == HttpStatusCode.Unauthorized) return Result.Failure(Error.Unauthorized);
            else if (response.StatusCode == HttpStatusCode.NotFound) return Result.Failure(Error.NotFound);
            else
            {
                logger.LogError("Unexpected response from Tracking Service: {StatusCode}", response.StatusCode);
                return Result.Failure(CommentErrors.TrackingServiceUnavailable);
            }
        }
        catch (TaskCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex,
                "Tracking service unavailable while validating membership for Issue {IssueId}",
                issueId);
            return Result<Guid>.Failure(CommentErrors.TrackingServiceUnavailable);
        }
    }
}
