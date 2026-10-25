namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;

public interface IReinspectionRequestRepository
{
    Task<ReinspectionRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ReinspectionRequest>> GetByOperatorAsync(Guid operatorId, CancellationToken ct = default);
    Task AddAsync(ReinspectionRequest request, CancellationToken ct = default);
    void Update(ReinspectionRequest request);
}
