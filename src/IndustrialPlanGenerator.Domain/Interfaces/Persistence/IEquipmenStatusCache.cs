namespace IndustrialPlanGenerator.Domain.Persistence;

using IndustrialPlanGenerator.Domain.Telemetry;

public interface IEquipmentStatusCache
{
    Task<EquipmentStatus?> GetAsync(Guid equipmentId, CancellationToken ct = default);
    Task UpsertAsync(EquipmentStatus status, CancellationToken ct = default);
    Task InvalidateAsync(Guid equipmentId, CancellationToken ct = default);
}