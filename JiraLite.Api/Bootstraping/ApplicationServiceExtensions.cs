using System;
using Asp.Versioning;
using JiraLite.Infrastructure.Data;
using Microsoft.Extensions.Options;

namespace JiraLite.Api.Bootstraping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

        builder.Services.AddApiVersioning(
            options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Version")
                );
            }
        );

        builder.Services.AddNpgsql<JiraLiteDbContext>(
            builder.Configuration.GetConnectionString("PostgreSqlConnection")
        );

        return builder;
    }
}
