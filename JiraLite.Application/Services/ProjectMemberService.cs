using System;
using JiraLite.Application.Interfaces;
using JiraLite.Infrastructure.Data;
using JiraLite.Infrastructure.Entities;
using JiraLite.Share.Common;
using JiraLite.Share.Dtos.Projects;
using JiraLite.Share.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JiraLite.Application.Services;

public class ProjectMemberService(JiraLiteDbContext dbContext, ILogger<ProjectMemberService> logger, IUserService userService) : IProjectMemberService
{
    private readonly JiraLiteDbContext _dbContext = dbContext;
    private readonly ILogger<ProjectMemberService> _logger = logger;
    private readonly IUserService _userService = userService;

    public async Task<Result> AddProjectMemberAsync(Guid projectId, AddProjectMemberRequest request, CancellationToken cancellationToken)
    {
        // Check project tồn tại?
        var projectExists = await _dbContext.Projects
            .AnyAsync(p => p.Id == projectId && p.IsActive, cancellationToken);
        if (!projectExists)
            return Result.Failure(ProjectErrors.ProjectNotFound);

        // Check user thuộc project chưa?
        var isProjectMemberExists = await _dbContext.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == request.UserId, cancellationToken);
        if (isProjectMemberExists)
        {
            return Result.Failure(ProjectErrors.ProjectMemberAlreadyExists);
        }

        var userInfo = await _userService.GetUserInfoAsync(request.UserId, cancellationToken);
        if (userInfo.IsFailure)
        {
            return Result.Failure(userInfo.Error);
        }

        var member = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = request.UserId,
            FullName = userInfo.Value!.FullName,
            Email = userInfo.Value.Email,
            Role = request.Role
        };

        _dbContext.ProjectMembers.Add(member);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Added user {UserId} to project {ProjectId} as {Role}", request.UserId, projectId, request.Role);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ProjectMemberDto>>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var projectExists = await _dbContext.Projects
            .AnyAsync(p => p.Id == projectId && p.IsActive, cancellationToken);
        if (!projectExists)
            return Result.Failure<IReadOnlyList<ProjectMemberDto>>(ProjectErrors.ProjectNotFound);

        IReadOnlyList<ProjectMemberDto> response = await _dbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.Project.IsActive)
            .Select(pm => new ProjectMemberDto
            {
                Id = pm.Id,
                FullName = pm.FullName,
                Email = pm.Email,
                Role = pm.Role.ToString()
            })
            .ToListAsync(cancellationToken);
        return Result.Success(response);
    }

    public async Task<Result> RemoveProjectMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        // Check project tồn tại?
        var projectExists = await _dbContext.Projects
            .AnyAsync(p => p.Id == projectId && p.IsActive, cancellationToken);
        if (!projectExists)
            return Result.Failure(ProjectErrors.ProjectNotFound);

        // Check user thuộc project chưa?
        var projectMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, cancellationToken);
        if (projectMember == null)
        {
            return Result.Failure(ProjectErrors.ProjectMemberNotFound);
        }

        _dbContext.ProjectMembers.Remove(projectMember);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed user {UserId} from project {ProjectId}", userId, projectId);
        return Result.Success();
    }

    public async Task<Result> UpdateProjectMemberRoleAsync(Guid projectId, Guid userId, UpdateProjectMemberRoleRequest request, CancellationToken cancellationToken)
    {
        // Check project tồn tại?
        var projectExists = await _dbContext.Projects
            .AnyAsync(p => p.Id == projectId && p.IsActive, cancellationToken);
        if (!projectExists)
            return Result.Failure(ProjectErrors.ProjectNotFound);

        // Check user thuộc project chưa?
        var projectMember = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, cancellationToken);
        if (projectMember == null)
        {
            return Result.Failure(ProjectErrors.ProjectMemberNotFound);
        }

        projectMember!.Role = request.Role;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated user {UserId} role in project {ProjectId} to {Role}", userId, projectId, request.Role);
        return Result.Success();
    }


}
