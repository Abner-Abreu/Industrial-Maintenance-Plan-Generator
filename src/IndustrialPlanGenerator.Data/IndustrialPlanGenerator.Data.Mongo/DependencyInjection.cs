namespace IndustrialPlanGenerator.Data.Mongo;

using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Mongo.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

public static class DependencyInjection
{
    public static IServiceCollection AddMongoData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mongo") 
            ?? throw new InvalidOperationException("Connection string 'Mongo' not found.");

        var databaseName = configuration["Mongo:Database"] 
            ?? throw new InvalidOperationException("Mongo database name not configured.");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddScoped<MongoDbContext>(sp => 
            new MongoDbContext(sp.GetRequiredService<IMongoClient>(), databaseName));
        
        services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
        services.AddScoped<IEquipmentStatusCache, EquipmentStatusCache>();

        return services;
    }
}