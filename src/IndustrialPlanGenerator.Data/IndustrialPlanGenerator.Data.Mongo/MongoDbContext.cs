namespace IndustrialPlanGenerator.Data.Mongo;

using IndustrialPlanGenerator.Domain.Telemetry;
using MongoDB.Driver;

public class MongoDbContext
{
    public IMongoCollection<SensorReading> SensorReadings => _database.GetCollection<SensorReading>("sensor_readings");
    public IMongoCollection<EquipmentStatus> EquipmentStatuses => _database.GetCollection<EquipmentStatus>("equipment_statuses");
    
    private readonly IMongoDatabase _database;
    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }
}