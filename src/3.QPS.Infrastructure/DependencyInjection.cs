using Microsoft.Extensions.DependencyInjection;
using QPS.Application.Interfaces;
using QPS.Infrastructure.Database;
using QPS.Infrastructure.Identity;
using QPS.Infrastructure.IoT;
using QPS.Infrastructure.Sms;

namespace QPS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDbContext, AppDbContext>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services.AddScoped<IMqttService, MqttClientService>();
        services.AddScoped<ISmsService, SmsService>();

        return services;
    }
}