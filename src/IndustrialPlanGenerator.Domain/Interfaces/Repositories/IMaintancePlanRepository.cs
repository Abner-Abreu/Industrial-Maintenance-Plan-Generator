namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;

public interface IMaintenancePlanRepository
{
    Task<MaintenancePlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MaintenancePlan?> GetWithParametersAndTasksAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MaintenancePlan>> GetByLineAsync(Guid lineId, CancellationToken ct = default);
    Task<IReadOnlyList<MaintenancePlan>> GetByTechnicianCreatorAsync(Guid technicianId, CancellationToken ct = default);

    Task AddAsync(MaintenancePlan plan, CancellationToken ct = default);
    void Update(MaintenancePlan plan);
    void Remove(MaintenancePlan plan);
}