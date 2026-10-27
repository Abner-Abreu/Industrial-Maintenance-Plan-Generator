using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Data.Postgres;
using Microsoft.EntityFrameworkCore;

public class MaintenanceTaskRepository : IMaintenanceTaskRepository
{
    private readonly AppDbContext _context;

    public MaintenanceTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    // RI-29
    public async Task<bool> IsActiveTaskDuplicatedAsync(
        string description, Guid equipmentId,
        MaintenanceType type, Criticality criticality,
        CancellationToken ct = default)
    {
        return await _context.Tasks
            .AnyAsync(t =>
                t.Description == description &&
                t.EquipmentSystem.Id == equipmentId &&
                t.Type == type &&
                t.Criticality == criticality &&
                t.Status == IndustrialPlanGenerator.Domain.Entities.TaskStatus.Active);
    }

    // RF-08 Most return active tasks only
    public async Task<IReadOnlyList<MaintenanceTask>> FindAsync(
        Guid? equipmentId, Guid? lineId,
        MaintenanceType? type, Criticality? criticality,
        CancellationToken ct = default)
    {
        var query = _context.Tasks
            .Where(t => t.Status == IndustrialPlanGenerator.Domain.Entities.TaskStatus.Active);

        if (equipmentId.HasValue)
        {
            query = query.Where(t => t.EquipmentSystem.Id == equipmentId);
        }

        if (lineId.HasValue)
        {
            query = query.Where(t => t.EquipmentSystem.ProductionLine.Id == lineId);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type);
        }

        if (criticality.HasValue)
        {
            query = query.Where(t => t.Criticality == criticality);
        }

        return await query.ToListAsync(ct);
    }

    //Used to find all tasks from a plan
    public async Task<IReadOnlyList<MaintenanceTask>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await _context.Tasks
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(MaintenanceTask task, CancellationToken ct = default)
    {
        await _context.Tasks.AddAsync(task);
    }

    public void Update(MaintenanceTask task)
    {
        _context.Tasks.Update(task);
    }

    public void Remove(MaintenanceTask task)
    {
        _context.Tasks.Remove(task);
    }
}