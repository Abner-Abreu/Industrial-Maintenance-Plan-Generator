namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class TechnicianRepository : ITechnicianRepository
{
    private readonly AppDbContext _context;

    public TechnicianRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Technician?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Technicians
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Technician?> GetWithLineAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Technicians
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<bool> IsAuthorizedInLineAsync(Guid technicianId, Guid lineId, TechnicianPermission permission, CancellationToken ct = default)
    {
        var technician = await _context.Technicians
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == technicianId, ct);

        if (technician == null)
            return false;

        return technician.Lines.Any(l => l.LineId == lineId && l.Permission == permission);
    }

    public async Task<IReadOnlyList<Technician>> GetByLineAsync(Guid lineId, CancellationToken ct = default)
    {
        return await _context.Technicians
            .Include(t => t.Lines)
            .Where(t => t.Lines.Any(l => l.LineId == lineId))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Technician technician, CancellationToken ct = default)
    {
        _context.Technicians.Add(technician);
        await _context.SaveChangesAsync(ct);
    }

    public void Update(Technician technician)
    {
        _context.Technicians.Update(technician);
    }

    public void Remove(Technician technician)
    {
        _context.Technicians.Remove(technician);
    }
}
