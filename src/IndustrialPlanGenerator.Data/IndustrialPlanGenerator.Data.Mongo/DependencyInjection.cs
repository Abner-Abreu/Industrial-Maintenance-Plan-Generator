namespace IndustrialPlanGenerator.Data.Mongo;

using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Domain.Telemetry;
using IndustrialPlanGenerator.Data.Mongo.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        // Register index creation as a hosted service
        services.AddHostedService<MongoIndexInitializer>();

        return services;
    }
}

// Hosted service to create MongoDB indexes on application startup.
internal sealed class MongoIndexInitializer : IHostedService
{
    private readonly IMongoClient _client;
    private readonly string _databaseName;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(IMongoClient client, IConfiguration configuration, ILogger<MongoIndexInitializer> logger)
    {
        _client = client;
        _databaseName = configuration["Mongo:Database"] 
            ?? throw new InvalidOperationException("Mongo database name not configured.");
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var database = _client.GetDatabase(_databaseName);

        // Sensor Readings Collection Indexes
        var sensorReadings = database.GetCollection<SensorReading>("sensor_readings");
        
        // Compound index for equipment + time range queries (RF-25)
        var equipmentTimeIndex = new CreateIndexModel<SensorReading>(
            Builders<SensorReading>.IndexKeys
                .Ascending(r => r.EquipmentId)
                .Ascending(r => r.Timestamp),
            new CreateIndexOptions { Name = "ix_sensor_reading_equipment_timestamp" });

        // Compound index for line + time range queries (RF-25)
        var lineTimeIndex = new CreateIndexModel<SensorReading>(
            Builders<SensorReading>.IndexKeys
                .Ascending(r => r.LineId)
                .Ascending(r => r.Timestamp),
            new CreateIndexOptions { Name = "ix_sensor_reading_line_timestamp" });

        // Compound index for sensor + time (GetLastBySensorAsync)
        var sensorTimeIndex = new CreateIndexModel<SensorReading>(
            Builders<SensorReading>.IndexKeys
                .Ascending(r => r.SensorId)
                .Descending(r => r.Timestamp),
            new CreateIndexOptions { Name = "ix_sensor_reading_sensor_timestamp" });

        // Compound index for type + time range queries (aggregation by type)
        var typeTimeIndex = new CreateIndexModel<SensorReading>(
            Builders<SensorReading>.IndexKeys
                .Ascending(r => r.Type)
                .Ascending(r => r.Timestamp),
            new CreateIndexOptions { Name = "ix_sensor_reading_type_timestamp" });

        // Unique index on sensor_id + timestamp (RI-38)
        var uniqueSensorTimeIndex = new CreateIndexModel<SensorReading>(
            Builders<SensorReading>.IndexKeys
                .Ascending(r => r.SensorId)
                .Ascending(r => r.Timestamp),
            new CreateIndexOptions { Name = "ux_sensor_reading_sensor_timestamp", Unique = true });

        await sensorReadings.Indexes.CreateManyAsync(
            new[] { equipmentTimeIndex, lineTimeIndex, sensorTimeIndex, typeTimeIndex, uniqueSensorTimeIndex },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Created indexes for sensor_readings collection");

        // Equipment Status Collection Indexes
        var equipmentStatuses = database.GetCollection<EquipmentStatus>("equipment_statuses");

        // Unique index on equipment_id (one status per equipment)
        var equipmentIdIndex = new CreateIndexModel<EquipmentStatus>(
            Builders<EquipmentStatus>.IndexKeys.Ascending(s => s.EquipmentId),
            new CreateIndexOptions { Name = "ux_equipment_status_equipment_id", Unique = true });

        // TTL index on LastUpdated for automatic cleanup of stale cache entries (e.g., 24 hours)
        var ttlIndex = new CreateIndexModel<EquipmentStatus>(
            Builders<EquipmentStatus>.IndexKeys.Ascending(s => s.LastUpdated),
            new CreateIndexOptions { Name = "ix_equipment_status_last_updated_ttl", ExpireAfter = TimeSpan.FromHours(24) });

        await equipmentStatuses.Indexes.CreateManyAsync(
            new[] { equipmentIdIndex, ttlIndex },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Created indexes for equipment_statuses collection");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}