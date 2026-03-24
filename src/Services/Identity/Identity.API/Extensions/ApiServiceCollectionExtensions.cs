using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using JiraLite.Shared.Contracts.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // 1. Cấu hình OpenAPI
        services.AddOpenApi();

        // 2. Cấu hình API Versioning
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Version")
            );
        });

        // 3. Cấu hình JSON (Enum -> String)
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // 4. Cấu hình JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((jwtOptions, settings) =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Value.Issuer,
                    ValidAudience = settings.Value.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddProblemDetails();

        // 5. Cấu hình Authorization Policies
        services.AddAuthorization(options =>
        {
            // Chỉ Admin (SystemRole = Admin trong JWT claim "role")
            options.AddPolicy("AdminOnly", policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Admin"));
        });

        return services;
    }
}