namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Postgres;
using Microsoft.EntityFrameworkCore;

public class ReinspectionRequestRepository : IReinspectionRequestRepository
{
    private readonly AppDbContext _context;

    public ReinspectionRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReinspectionRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReinspectionRequests
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<ReinspectionRequest>> GetByOperatorAsync(Guid operatorId, CancellationToken ct = default)
    {
        return await _context.ReinspectionRequests
            .Where(r => r.OperatorId == operatorId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ReinspectionRequest request, CancellationToken ct = default)
    {
        await _context.ReinspectionRequests.AddAsync(request);
    }

    public void Update(ReinspectionRequest request)
    {
        _context.ReinspectionRequests.Update(request);
    }
}