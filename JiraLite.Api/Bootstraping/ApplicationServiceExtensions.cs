using System;
using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using JiraLite.Application.Interfaces;
using JiraLite.Application.Services;
using JiraLite.Infrastructure.Data;
using JiraLite.Share.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

        // Tự động parse tất cả Enum thành String trong toàn bộ hệ thống
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("JwtSettings"));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtSettings = builder.Services.BuildServiceProvider()
                    .GetRequiredService<IOptions<JwtSettings>>()
                    .Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        builder.Services.AddHttpClient<IUserService, UserService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["AuthApi:BaseUrl"]!);
            client.DefaultRequestHeaders.Add("X-Api-Key", builder.Configuration["AuthApi:ApiKey"]!);
        });
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
        builder.Services.AddScoped<IProjectIssuesServices, ProjectIssuesServices>();
        builder.Services.AddScoped<IIssueService, IssueService>();
        builder.Services.AddScoped<ILogService, DbLogService>();

        return builder;
    }
}
