using System;
using MediatR;
using Tracking.Domain.Errors;
using Tracking.Domain.Interfaces;

namespace Tracking.Application.Features.Projects;

public static class DeactivateProject
{
    public record Command(Guid ProjectId, Guid CurrentUserId) : IRequest<Result>;
    public class Handler(IProjectRepository repo, IUnitOfWork uow) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command cmd, CancellationToken ct = default)
        {
            var project = await repo.GetProjectByIdAsync(cmd.ProjectId, ct);
            if (project is null)
                return Result.Failure(ProjectErrors.ProjectNotFound);

            project.IsActive = false;
            await uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
