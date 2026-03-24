using System;
using FluentValidation;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using MediatR;

namespace Identity.Application.Features.Users;

public static class LockUser
{
    public record class Command(Guid UserId) : IRequest<Result>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    public class Handler(IUserRepository userRepo, IUnitOfWork uow) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command cmd, CancellationToken ct = default)
        {
            var user = await userRepo.GetUserByIdAsync(cmd.UserId, ct, true);
            if (user is null)
                return Result.Failure(UserErrors.UserNotFound);
            if (!user.IsActive)
                return Result.Failure(UserErrors.UserAlreadyLocked);

            user.IsActive = false;

            await uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
