namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class TaskExecutionRepository : ITaskExecutionRepository
{
    private readonly AppDbContext _context;

    public TaskExecutionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskExecution?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TaskExecutions
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<TaskExecution>> GetByAssigmentAsync(Guid assignmentId, CancellationToken ct = default)
    {
        return await _context.TaskExecutions
            .Where(t => t.AssignmentId == assignmentId)
            .ToListAsync(ct);
    }

    // RI-34
    public async Task<bool> IsThereUnfulfilledExecutionAsync(Guid executionId, CancellationToken ct = default)
    {
        return await _context.TaskExecutions
            .AnyAsync(t => t.Id == executionId && !t.Completed, ct);
    }

    public async Task AddAsync(TaskExecution execution, CancellationToken ct = default)
    {
        await _context.TaskExecutions.AddAsync(execution, ct);
    }

    public void Update(TaskExecution execution)
    {
        _context.TaskExecutions.Update(execution);
    }
}