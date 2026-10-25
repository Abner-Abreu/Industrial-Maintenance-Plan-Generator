namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;

public interface IOperatorRepository
{
    Task<Operator?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Operator?> GetWithShiftAndLinesAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Operator>> GetByLineAsync(Guid lineId, CancellationToken ct = default);
    Task AddAsync(Operator Operator, CancellationToken ct = default);
    void Update(Operator Operator);
    void Remove(Operator Operator);
}