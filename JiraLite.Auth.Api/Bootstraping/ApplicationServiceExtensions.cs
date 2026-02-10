using System;
using JiraLite.Auth.Api.Services;
using JiraLite.Auth.Infrastructure.Data;
using JiraLite.Auth.Infrastructure.Entities;
using JiraLite.Share.Settings;
using Microsoft.AspNetCore.Identity;

namespace JiraLite.Auth.Api.Bootstraping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("JwtSettings"));

        builder.Services.AddNpgsql<AuthDbContext>(
            builder.Configuration.GetConnectionString("PostgreSqlConnection")
        );

        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();


        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        return builder;
    }
}
