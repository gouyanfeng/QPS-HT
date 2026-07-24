using Microsoft.Extensions.DependencyInjection;
using QPS.Application.Interfaces;
using QPS.Infrastructure.Database;
using QPS.Infrastructure.Identity;

namespace QPS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDbContext, AppDbContext>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
