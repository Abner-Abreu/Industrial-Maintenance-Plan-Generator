namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;

public interface ITaskExecutionRepository
{
    Task<TaskExecution?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TaskExecution>> GetByAssigmentAsync(Guid assignmentId, CancellationToken ct = default);

    // RI-34
    Task<bool> IsThereUnfulfilledExecutionAsync(Guid executionId, CancellationToken ct = default);

    Task AddAsync(TaskExecution execution, CancellationToken ct = default);
    void Update(TaskExecution execution);
}