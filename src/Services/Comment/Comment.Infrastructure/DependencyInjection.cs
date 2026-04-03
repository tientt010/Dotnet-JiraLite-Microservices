using System;
using Comment.Application.Interfaces;
using Comment.Domain.Interfaces;
using Comment.Infrastructure.Data;
using Comment.Infrastructure.Repositories;
using Comment.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

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
        services.AddHttpClient<ITrackingService, TrackingServiceClient>(client =>
        {
            var baseUrl = configuration["Services:Tracking:BaseUrl"]
                ?? throw new InvalidOperationException("Services:Tracking:BaseUrl is required");

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    // Nếu API bị lỗi tự động gọi lại (Tối đa 2 lần)
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * attempt));
    }

    // Nếu API bị lỗi liên tục, ngắt kết nối trong 30 giây để tránh làm quá tải hệ thống
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
