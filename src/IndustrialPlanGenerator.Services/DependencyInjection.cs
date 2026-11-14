using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IndustrialPlanGenerator.Services.Contracts.Auth;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Catalogs;
using IndustrialPlanGenerator.Services.EquipmentStatus;
using IndustrialPlanGenerator.Services.Lines;
using IndustrialPlanGenerator.Services.Sensors;
using IndustrialPlanGenerator.Services.Systems;
using IndustrialPlanGenerator.Services.Users;

namespace IndustrialPlanGenerator.Services;

/// <summary>Registration of the Application layer services in the dependency injection container.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Application layer services. Each team member adds their services here,
    /// for example: <c>services.AddScoped&lt;IAuthService, AuthService&gt;();</c>
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILineAccessPolicy, LineAccessPolicy>();
        services.TryAddScoped<ILineService, LineServiceStub>();
        services.TryAddScoped<IEquipmentSystemService, EquipmentSystemServiceStub>();
        services.TryAddScoped<ISensorService, SensorServiceStub>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.TryAddScoped<IEquipmentStatusService, EquipmentStatusServiceStub>();


        return services;
    }
}
