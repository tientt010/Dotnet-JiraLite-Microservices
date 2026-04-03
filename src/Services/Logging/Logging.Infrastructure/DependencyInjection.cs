using Logging.Application.Interfaces;
using Logging.Domain.Interfaces;
using Logging.Infrastructure.Data;
using Logging.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logging.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LoggingDb")
            ?? throw new InvalidOperationException("ConnectionStrings:LoggingDb is required");

        services.AddDbContext<LoggingDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<ILoggingReadDbContext>(sp => sp.GetRequiredService<LoggingDbContext>());

        return services;
    }
}


