using System;
using Comment.Application.Interfaces;
using Comment.Domain.Interfaces;
using Comment.Infrastructure.Data;
using Comment.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CommentDb")
            ?? throw new InvalidOperationException("ConnectionStrings:CommentDb is required");

        services.AddDbContext<CommentDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                npgsqlOptions.CommandTimeout(30);
            });

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICommentReadDbContext>(sp => sp.GetRequiredService<CommentDbContext>());

        return services;
    }
}
