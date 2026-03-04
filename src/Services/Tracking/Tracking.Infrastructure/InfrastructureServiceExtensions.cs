using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracking.Domain.Interfaces;
using Tracking.Infrastructure.Data;
using Tracking.Infrastructure.Repositories;

namespace Tracking.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Đăng ký DbContext với PostgreSQL
        services.AddDbContext<TrackingDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IProjectRepository, ProjectRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
