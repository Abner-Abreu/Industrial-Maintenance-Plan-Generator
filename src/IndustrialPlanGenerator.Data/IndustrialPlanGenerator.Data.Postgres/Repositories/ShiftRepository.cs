namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class ShiftRepository : IShiftRepository
{
    private readonly AppDbContext _context;

    public ShiftRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Shift?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Shifts
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<Shift>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Shifts
            .ToListAsync(ct);
    }

    public async Task AddAsync(Shift shift, CancellationToken ct = default)
    {
        await _context.Shifts.AddAsync(shift, ct);
    }

    public void Update(Shift shift)
    {
        _context.Shifts.Update(shift);
    }

    public void Remove(Shift shift)
    {
        _context.Shifts.Remove(shift);
    }
}