namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;

public interface IEquipmentSystemRepository
{
    Task<EquipmentSystem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<EquipmentSystem>> GetByLineaAsync(Guid lineId, CancellationToken ct = default);
    Task AddAsync(EquipmentSystem equipment, CancellationToken ct = default);
    void Update(EquipmentSystem equipment);
    void Remove(EquipmentSystem equipment);
}