namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Postgres;
using Microsoft.EntityFrameworkCore;

public class OperatorRepository : IOperatorRepository
{
    private readonly AppDbContext _context;

    public OperatorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Operator?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Operators.FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Operator?> GetWithShiftAndLinesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Operators
            .Include(o => o.Shift)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IReadOnlyList<Operator>> GetByLineAsync(Guid lineId, CancellationToken ct = default)
    {
        return await _context.Operators
            .Include(o => o.Lines)
            .Where(o => o.Lines.Any(l => l.LineId == lineId))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Operator Operator, CancellationToken ct = default)
    {
        await _context.Operators.AddAsync(Operator,ct);
    }

    public void Update(Operator Operator)
    {
        _context.Operators.Update(Operator);
    }

    public void Remove(Operator Operator)
    {
        _context.Operators.Remove(Operator);
    }
}