namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Postgres;
using Microsoft.EntityFrameworkCore;

public class EquipmentSystemRepository : IEquipmentSystemRepository
{
    private AppDbContext _db;

    public EquipmentSystemRepository(AppDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task AddAsync(EquipmentSystem equipment, CancellationToken ct = default)
    {
        await _db.EquipmentSystems.AddAsync(equipment);
    }

    public async Task<EquipmentSystem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.EquipmentSystems.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyList<EquipmentSystem>> GetByLineaAsync(Guid lineId, CancellationToken ct = default)
    {
        return await _db.EquipmentSystems
            .Where(e => e.LineId == lineId)
            .ToListAsync(ct);
    }

    public void Remove(EquipmentSystem equipment)
    {
        _db.EquipmentSystems.Remove(equipment);
    }

    public void Update(EquipmentSystem equipment)
    {
        _db.EquipmentSystems.Update(equipment);
    }
}