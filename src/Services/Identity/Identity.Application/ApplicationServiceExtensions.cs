using System;
using Microsoft.Extensions.DependencyInjection;
namespace Identity.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly);
        });
        return services;
    }
}
