using Microsoft.Extensions.DependencyInjection;
using IndustrialPlanGenerator.Services.Contracts.Auth;
using IndustrialPlanGenerator.Services.Auth;

namespace IndustrialPlanGenerator.Services;

/// <summary>Registro en el contenedor de inyección de dependencias de los servicios de la capa Application.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa Application. Cada integrante añade aquí sus servicios,
    /// por ejemplo: <c>services.AddScoped&lt;IAuthService, AuthService&gt;();</c>
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();


        return services;
    }
}