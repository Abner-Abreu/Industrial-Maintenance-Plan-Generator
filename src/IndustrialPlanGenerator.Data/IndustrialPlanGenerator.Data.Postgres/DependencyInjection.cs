namespace IndustrialPlanGenerator.Data.Postgres;

using IndustrialPlanGenerator.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgresData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres") 
            ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                connectionString, 
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}