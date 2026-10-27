namespace IndustrialPlanGenerator.Data.Mongo.Repositories;

using IndustrialPlanGenerator.Domain.Telemetry;
using IndustrialPlanGenerator.Domain.Persistence;
using MongoDB.Driver;

public class EquipmentStatusCache : IEquipmentStatusCache
{
    private readonly IMongoCollection<EquipmentStatus> _collection;

    public EquipmentStatusCache(MongoDbContext context)
    {
        _collection = context.EquipmentStatuses;
    }

    public async Task<EquipmentStatus?> GetAsync(Guid equipmentId, CancellationToken ct = default)
    {
        var filter = Builders<EquipmentStatus>.Filter.Eq(s => s.EquipmentId, equipmentId);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task UpsertAsync(EquipmentStatus status, CancellationToken ct = default)
    {
        var filter = Builders<EquipmentStatus>.Filter.Eq(s => s.EquipmentId, status.EquipmentId);
        var options = new ReplaceOptions { IsUpsert = true };
        await _collection.ReplaceOneAsync(filter, status, options, ct);
    }

    public async Task InvalidateAsync(Guid equipmentId, CancellationToken ct = default)
    {
        var filter = Builders<EquipmentStatus>.Filter.Eq(s => s.EquipmentId, equipmentId);
        await _collection.DeleteOneAsync(filter, ct);
    }
}