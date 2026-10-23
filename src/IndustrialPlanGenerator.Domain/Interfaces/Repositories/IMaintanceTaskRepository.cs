namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;

public interface IMaintenanceTaskRepository
{
    Task<MaintenanceTask?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // RI-29
    Task<bool> IsActiveTaskDuplicatedAsync(
        string description, Guid equipmentId,
        MaintenanceType type, Criticality criticality,
        CancellationToken ct = default);

    // RF-08 Most return active tasks only
    Task<IReadOnlyList<MaintenanceTask>> FindAsync(
        Guid? equipmentId, Guid? lineId,
        MaintenanceType? type, Criticality? criticality,
        CancellationToken ct = default);

    //Used to find all tasks from a plan
    Task<IReadOnlyList<MaintenanceTask>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task AddAsync(MaintenanceTask task, CancellationToken ct = default);
    void Update(MaintenanceTask task);
    void Remove(MaintenanceTask task);
}