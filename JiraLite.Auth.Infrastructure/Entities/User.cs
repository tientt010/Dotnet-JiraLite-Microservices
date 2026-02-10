using System;
using JiraLite.Share.Enums;

namespace JiraLite.Auth.Infrastructure.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public SystemRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

}

