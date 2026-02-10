using System;

namespace JiraLite.Share.Settings;

public class JwtSettings
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int AccessTokenExpiryInMinutes { get; init; } = 15;
    public int RefreshTokenExpiryInDays { get; init; } = 30;
}
