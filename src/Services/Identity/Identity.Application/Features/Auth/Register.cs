using System;
using FluentValidation;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using Identity.Domain.Interfaces;
using MediatR;

namespace Identity.Application.Features.Auth;

public class Register
{
    public record Command(string Email, string Password, string FullName) : IRequest<Result>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        }
    }
    public class Handler(IUserRepository repo, IPasswordHasher passwordHasher, IUnitOfWork uow) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command cmd, CancellationToken ct = default)
        {
            bool isExistingUser = await repo.CheckEmailExistsAsync(cmd.Email, ct);
            if (isExistingUser)
                return Result.Failure(AuthErrors.EmailAlreadyInUse);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = cmd.Email,
                FullName = cmd.FullName,
                PasswordHash = string.Empty,
                Role = SystemRole.User,
                IsActive = true
            };

            user.PasswordHash = passwordHasher.HashPassword(user, cmd.Password);

            repo.Add(user);

            await uow.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
