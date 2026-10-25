namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Postgres;
using Microsoft.EntityFrameworkCore;

public class MaintenancePlanRepository : IMaintenancePlanRepository
{
    private AppDbContext _context;

    public MaintenancePlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenancePlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Plans
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<MaintenancePlan?> GetWithParametersAndTasksAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Plans
            .Include(t => t.PlanParameters)
            .Include(t => t.PlanTasks)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<MaintenancePlan>> GetByLineAsync(Guid lineId, CancellationToken ct = default)
    {
        return await _context.Plans
            .Where(t => t.LineId == lineId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MaintenancePlan>> GetByTechnicianCreatorAsync(Guid technicianId, CancellationToken ct = default)
    {
        return await _context.Plans
            .Where(t => t.TechnicianCreatorId == technicianId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(MaintenancePlan plan, CancellationToken ct = default)
    {
        await _context.Plans.AddAsync(plan);
    }

    public void Update(MaintenancePlan plan)
    {
        _context.Plans.Update(plan);
    }

    public void Remove(MaintenancePlan plan)
    {
        _context.Plans.Remove(plan);
    }
}