using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Application.Common;
using SaaS.Application.Keycloak;
using SaaS.Application.Provisioning;
using SaaS.Application.Tenants;
using SaaS.Infrastructure.Keycloak;
using SaaS.Infrastructure.Provisioning;
using SaaS.Infrastructure.Messaging;
using SaaS.Infrastructure.Persistence;
using SaaS.Infrastructure.TenantContext;
using SaaS.Infrastructure.Tenants;

namespace SaaS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=saas;Username=postgres;Password=postgres";

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITenantContext, HttpTenantContext>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserTenantRepository, UserTenantRepository>();
        services.AddScoped<IUserTermsAcceptanceRepository, UserTermsAcceptanceRepository>();
        services.AddScoped<IUserProvisioningService, UserProvisioningService>();
        services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();

        services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>();

        return services;
    }
}
