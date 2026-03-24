using System;
using FluentValidation;
using Identity.Application.Behaviors;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthSessionService, AuthSessionService>();
        services.AddMediatR(config =>
        {
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);


        return services;
    }
}
