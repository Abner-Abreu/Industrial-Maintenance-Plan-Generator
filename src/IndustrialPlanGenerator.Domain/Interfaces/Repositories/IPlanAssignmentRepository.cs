namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;

public interface IPlanAssignmentRepository
{
    Task<PlanAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PlanAssignment>> GetByOperatorAsync(Guid operatorId, CancellationToken ct = default);
    Task<IReadOnlyList<PlanAssignment>> GetByPlanAsync(Guid planId, CancellationToken ct = default);

    // RI-35
    Task<bool> IsThereAssignmentAsync(Guid planId, Guid operatorId, CancellationToken ct = default);
    
    Task AddAsync(PlanAssignment assignment, CancellationToken ct = default);
    void Update(PlanAssignment assignment);
    void Remove(PlanAssignment assignment);
}
