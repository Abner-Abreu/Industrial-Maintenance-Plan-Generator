namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;

public interface IProductionLineRepository
{
    Task<ProductionLine?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductionLine?> GetWithEquipmentAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionLine>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ProductionLine line, CancellationToken ct = default);
    void Update(ProductionLine line);
    void Remove(ProductionLine line);
}