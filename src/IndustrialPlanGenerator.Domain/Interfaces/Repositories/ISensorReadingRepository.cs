namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Telemetry;

public interface ISensorReadingRepository
{
    Task AppendAsync(SensorReading reading, CancellationToken ct = default);
    Task AppendRangeAsync(IEnumerable<SensorReading> readings, CancellationToken ct = default);

    Task<IReadOnlyList<SensorReading>> GetByEquipmentAsync(Guid equipmentId, DateTime from, DateTime till, CancellationToken ct = default);
    
    Task<IReadOnlyList<SensorReadingAggregate>> GetAggregateAsync(
        Guid? equipmentId, Guid? lineId, DateTime from, DateTime till,
        TimeSpan bucket, CancellationToken ct = default);

    Task<SensorReading?> GetLastBySensorAsync(Guid sensorId, CancellationToken ct = default);
}