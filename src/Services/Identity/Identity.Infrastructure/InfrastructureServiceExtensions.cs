using System;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Repositories;
using JiraLite.Shared.Contracts.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Đăng ký DbContext với PostgreSQL
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenHasher, TokenHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }
}
