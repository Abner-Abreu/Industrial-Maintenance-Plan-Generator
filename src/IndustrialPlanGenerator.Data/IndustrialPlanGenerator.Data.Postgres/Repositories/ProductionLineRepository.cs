namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Postgres;
using Microsoft.EntityFrameworkCore;

public class ProductionLineRepository : IProductionLineRepository
{
    private readonly AppDbContext _context;

    public ProductionLineRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<ProductionLine?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _context.ProductionLines
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public Task<ProductionLine?> GetWithEquipmentAsync(Guid id, CancellationToken ct = default)
    {
        return _context.ProductionLines
            .Include(l => l.EquipmentSystems)
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<IReadOnlyList<ProductionLine>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ProductionLines
            .ToListAsync(ct);
    }

    public async Task AddAsync(ProductionLine line, CancellationToken ct = default)
    {
        await _context.ProductionLines.AddAsync(line);
        await _context.SaveChangesAsync(ct);
    }

    public void Update(ProductionLine line)
    {
        _context.ProductionLines.Update(line);
        _context.SaveChanges();
    }

    public void Remove(ProductionLine line)
    {
        _context.ProductionLines.Remove(line);
        _context.SaveChanges();
    }
}