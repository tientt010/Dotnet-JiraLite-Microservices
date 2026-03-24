using System;
using FluentValidation;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using MediatR;

namespace Identity.Application.Features.Users;

public static class UpdateProfile
{
    public record Command(Guid UserId, string? FullName, string? AvatarUrl) : IRequest<Result>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.FullName).MaximumLength(100);
            RuleFor(x => x.AvatarUrl).MaximumLength(200);
        }
    }

    public class Handler(IUserRepository userRepo, IUnitOfWork uow) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command cmd, CancellationToken ct = default)
        {
            var user = await userRepo.GetUserByIdAsync(cmd.UserId, ct, true);
            if (user is null)
                return Result.Failure(UserErrors.UserNotFound);

            if (!string.IsNullOrEmpty(cmd.FullName))
                user.FullName = cmd.FullName;

            user.AvatarUrl = cmd.AvatarUrl;

            await uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
