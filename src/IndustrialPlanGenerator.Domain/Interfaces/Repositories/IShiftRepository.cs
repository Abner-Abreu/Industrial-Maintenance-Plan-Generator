namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;

public interface IShiftRepository
{
    Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Shift>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Shift shift, CancellationToken ct = default);
    void Update(Shift shift);
    void Remove(Shift shift);
}