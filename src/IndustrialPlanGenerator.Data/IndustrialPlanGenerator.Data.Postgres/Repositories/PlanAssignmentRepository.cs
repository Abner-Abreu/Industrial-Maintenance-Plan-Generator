using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Postgres;
using Microsoft.EntityFrameworkCore;

public class PlanAssignmentRepository : IPlanAssignmentRepository
{
    private readonly AppDbContext _context;

    public PlanAssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlanAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PlanAssignments
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IReadOnlyList<PlanAssignment>> GetByOperatorAsync(Guid operatorId, CancellationToken ct = default)
    {
        return await _context.PlanAssignments
        .Where(a => a.OperatorId == operatorId)
        .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlanAssignment>> GetByPlanAsync(Guid planId, CancellationToken ct = default)
    {
        return await  _context.PlanAssignments
            .Where(a => a.PlanId == planId)
            .ToListAsync(ct);
    }

    // RI-35
    public async Task<bool> IsThereAssignmentAsync(Guid planId, Guid operatorId, CancellationToken ct = default)
    {
        return await _context.PlanAssignments
            .AnyAsync(a => a.PlanId == planId && a.OperatorId == operatorId, ct);
    }

    public async Task AddAsync(PlanAssignment assignment, CancellationToken ct = default)
    {
        await _context.PlanAssignments.AddAsync(assignment);
    }

    public void Update(PlanAssignment assignment)
    {
        _context.PlanAssignments.Update(assignment);
    }

    public void Remove(PlanAssignment assignment)
    {
        _context.PlanAssignments.Remove(assignment);
    }
}