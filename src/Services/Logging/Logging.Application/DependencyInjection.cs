using System.Reflection;
using FluentValidation;
using Logging.Application.Behaviors;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logging.Application;

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

        // Đăng ký MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumers(assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(config["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(config["RabbitMQ:Username"] ?? "guest");
                    h.Password(config["RabbitMQ:Password"] ?? "guest");

                    // Retry kết nối khi RabbitMQ chưa sẵn sàng
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
                });

                // Cho phép deserialize raw JSON messages
                cfg.UseRawJsonSerializer();

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
