namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;

public interface ITechnicianRepository
{
    Task<Technician?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Technician?> GetWithLineAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsAuthorizedInLineAsync(Guid technicianId, Guid lineaId, TechnicianPermission permiso, CancellationToken ct = default);
    Task<IReadOnlyList<Technician>> GetByLineAsync(Guid lineaId, CancellationToken ct = default);
    Task AddAsync(Technician technician, CancellationToken ct = default);
    void Update(Technician technician);
    void Remove(Technician technician);
}