using FluentValidation;
using JiraLite.Share.Common;
using Logging.Application.Dtos;
using Logging.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logging.Application.Feature.Logs;

public static class GetIssueLogsQuery
{
    public record Query(string IssueId, int PageIndex, int PageSize) : IRequest<Result<PaginationResponse<ActivityLogDto>>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.IssueId).NotEmpty();
            RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }

    public class Handler(ILoggingReadDbContext context) : IRequestHandler<Query, Result<PaginationResponse<ActivityLogDto>>>
    {
        public async Task<Result<PaginationResponse<ActivityLogDto>>> Handle(Query query, CancellationToken ct)
        {
            var skip = (query.PageIndex - 1) * query.PageSize;

            // 1. Tạo câu truy vấn gốc (chưa thực thi xuống DB)
            var baseQuery = context.ActivityLogs
                .Where(log => log.Target.Id == query.IssueId);

            // 2. Lấy tổng số lượng record
            var totalCount = await baseQuery.CountAsync(ct);

            // 3. Lấy dữ liệu chi tiết đã sắp xếp và phân trang
            var logs = await baseQuery
                .Include(log => log.Changes)
                .OrderByDescending(log => log.Timestamp)
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync(ct);

            var dtos = logs.Select(log => new ActivityLogDto
            {
                LogId = log.Id.ToString(),
                ActionType = log.ActionType.ToString(),
                Timestamp = log.Timestamp,
                Actor = new LogActorDto
                {
                    Id = log.Actor.Id,
                    Code = log.Actor.Code,
                    Name = log.Actor.Name,
                    AvatarUrl = log.Actor.AvatarUrl
                },
                Target = new LogTargetDto
                {
                    Type = log.Target.Type.ToString(),
                    Id = log.Target.Id,
                    Code = log.Target.Code,
                    Name = log.Target.Name
                },
                Changes = log.Changes.Select(c => new LogChangeDto
                {
                    Field = c.Field,
                    OldValue = c.OldValue,
                    OldCode = c.OldCode,
                    OldId = c.OldId,
                    NewValue = c.NewValue,
                    NewCode = c.NewCode,
                    NewId = c.NewId
                }).ToList()
            }).ToList();

            return Result<PaginationResponse<ActivityLogDto>>.Success(new PaginationResponse<ActivityLogDto>(
                query.PageIndex,
                query.PageSize,
                totalCount,
                dtos
            ));
        }
    }
}