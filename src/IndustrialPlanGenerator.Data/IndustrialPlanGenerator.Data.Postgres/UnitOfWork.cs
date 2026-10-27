namespace IndustrialPlanGenerator.Data.Postgres;

using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Postgres.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork
{
    public IEquipmentSystemRepository Equipments { get; }
    public IMaintenancePlanRepository Plans { get; }
    public IMaintenanceTaskRepository Tasks { get; }
    public IOperatorRepository Operators { get; }
    public IPlanAssignmentRepository PlanAssignments { get; }
    public IProductionLineRepository Line { get; }
    public IReinspectionRequestRepository ReinspectionRequests { get; }
    public IShiftRepository Shifts { get; }
    public ITaskExecutionRepository TaskExecutions { get; }
    public ITechnicianRepository Technicians { get; }
    public IUserRepository Users { get; }

    private readonly AppDbContext _db;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;

        Equipments = new EquipmentSystemRepository(_db);
        Plans = new MaintenancePlanRepository(_db);
        Tasks = new MaintenanceTaskRepository(_db);
        Operators = new OperatorRepository(_db);
        PlanAssignments = new PlanAssignmentRepository(_db);
        Line = new ProductionLineRepository(_db);
        ReinspectionRequests = new ReinspectionRequestRepository(_db);
        Shifts = new ShiftRepository(_db);
        TaskExecutions = new TaskExecutionRepository(_db);
        Technicians = new TechnicianRepository(_db);
        Users = new UserRepository(_db);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    // Transactions
    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}