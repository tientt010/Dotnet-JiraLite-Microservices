using System;
using System.Reflection;
using Comment.Application.Behaviors;
using Comment.Application.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Đăng ký FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Đăng ký MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        return services;
    }
}
