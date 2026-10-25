namespace IndustrialPlanGenerator.Data.Postgres;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<PlantManager> PlantManagers => Set<PlantManager>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<EquipmentSystem> EquipmentSystems => Set<EquipmentSystem>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<MaintenanceTask> Tasks => Set<MaintenanceTask>();
    public DbSet<MaintenancePlan> Plans => Set<MaintenancePlan>();
    public DbSet<PlanParameters> PlanParameters => Set<PlanParameters>();
    public DbSet<PlanTask> PlanTasks => Set<PlanTask>();
    public DbSet<PlanValidation> PlanValidations => Set<PlanValidation>();
    public DbSet<PlanAssignment> PlanAssignments => Set<PlanAssignment>();
    public DbSet<TaskExecution> TaskExecutions => Set<TaskExecution>();
    public DbSet<ReinspectionRequest> ReinspectionRequests => Set<ReinspectionRequest>();
    public DbSet<TechnicianLine> TechnicianLines => Set<TechnicianLine>();
    public DbSet<OperatorLine> OperatorLines => Set<OperatorLine>();

    protected override void ConfigureConventions(ModelConfigurationBuilder cb)
    {
        // All Guid -> uuid 
        cb.Properties<Guid>().HaveColumnType("uuid");
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}