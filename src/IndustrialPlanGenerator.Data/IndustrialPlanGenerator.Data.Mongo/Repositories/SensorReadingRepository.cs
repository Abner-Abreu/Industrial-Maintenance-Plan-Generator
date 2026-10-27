namespace IndustrialPlanGenerator.Data.Mongo.Repositories;

using IndustrialPlanGenerator.Domain.Telemetry;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Repositories;
using MongoDB.Driver;

public class SensorReadingRepository : ISensorReadingRepository
{
    private readonly IMongoCollection<SensorReading> _collection;

    public SensorReadingRepository(MongoDbContext context)
    {
        _collection = context.SensorReadings;
    }

    public async Task AppendAsync(SensorReading reading, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(reading, cancellationToken: ct);
    }

    public async Task AppendRangeAsync(IEnumerable<SensorReading> readings, CancellationToken ct = default)
    {
        await _collection.InsertManyAsync(readings, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<SensorReading>> GetByEquipmentAsync(Guid equipmentId, DateTime from, DateTime till, CancellationToken ct = default)
    {
        var filter = Builders<SensorReading>.Filter.And(
            Builders<SensorReading>.Filter.Eq(r => r.EquipmentId, equipmentId),
            Builders<SensorReading>.Filter.Gte(r => r.Timestamp, from),
            Builders<SensorReading>.Filter.Lte(r => r.Timestamp, till)
        );

        return await _collection.Find(filter)
            .SortBy(r => r.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SensorReadingAggregate>> GetAggregateAsync(
        Guid? equipmentId, Guid? lineId, DateTime from, DateTime till,
        TimeSpan bucket, CancellationToken ct = default)
    {
        var filter = Builders<SensorReading>.Filter.And(
            Builders<SensorReading>.Filter.Gte(r => r.Timestamp, from),
            Builders<SensorReading>.Filter.Lte(r => r.Timestamp, till)
        );

        if (equipmentId.HasValue)
        {
            filter = filter & Builders<SensorReading>.Filter.Eq(r => r.EquipmentId, equipmentId.Value);
        }

        if (lineId.HasValue)
        {
            filter = filter & Builders<SensorReading>.Filter.Eq(r => r.LineId, lineId.Value);
        }

        // For MongoDB aggregation, we'll use a simpler approach: fetch data and aggregate in memory
        // This is suitable for telemetry data which is typically queried in smaller time ranges
        var readings = await _collection.Find(filter).ToListAsync(ct);

        var bucketTicks = bucket.Ticks;
        var fromTicks = from.Ticks;

        return readings
            .GroupBy(r => new
            {
                r.EquipmentId,
                r.LineId,
                r.Type,
                Bucket = new DateTime(((r.Timestamp.Ticks - fromTicks) / bucketTicks) * bucketTicks + fromTicks)
            })
            .Select(g => new SensorReadingAggregate
            {
                EquipmentId = g.Key.EquipmentId,
                LineId = g.Key.LineId,
                Type = g.Key.Type,
                Bucket = g.Key.Bucket,
                Average = g.Average(r => r.Value),
                Min = g.Min(r => r.Value),
                Max = g.Max(r => r.Value),
                Count = g.Count()
            })
            .OrderBy(a => a.Bucket)
            .ToList();
    }

    public async Task<SensorReading?> GetLastBySensorAsync(Guid sensorId, CancellationToken ct = default)
    {
        var filter = Builders<SensorReading>.Filter.Eq(r => r.SensorId, sensorId);
        return await _collection.Find(filter)
            .SortByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync(ct);
    }
}