namespace IndustrialPlanGenerator.Domain.Persistence;

using IndustrialPlanGenerator.Domain.Repositories;
public interface IUnitOfWork
{
    IEquipmentSystemRepository Equipments {get;}
    IMaintenancePlanRepository Plans {get;}
    IMaintenanceTaskRepository Tasks {get;}
    IOperatorRepository Operators {get;}
    IPlanAssignmentRepository PlanAssignments {get;}
    IProductionLineRepository Line {get;}
    IReinspectionRequestRepository ReinspectionRequests {get;}
    IShiftRepository Shifts {get;}
    ITaskExecutionRepository TaskExecutions {get;}
    ITechnicianRepository Technicians {get;}
    IUserRepository Users {get;}
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}