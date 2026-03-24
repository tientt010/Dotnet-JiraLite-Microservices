using System;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Authentication;

public class PasswordHasher(IPasswordHasher<User> passwordHasher) : IPasswordHasher
{
    public string HashPassword(User user, string password)
    {
        return passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
    {
        return passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword) != PasswordVerificationResult.Failed;
    }
}
