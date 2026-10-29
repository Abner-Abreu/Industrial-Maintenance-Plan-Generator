namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class MaintenanceTask : Entity
{
    public required string Description { get; set; }
    public required Guid SystemId { get; set; }
    public required Criticality Criticality { get; set; }
    public required Guid TechnicianCreatorId { get; set; }
    public required DateOnly CreationDate { get; set; }
    public required TaskStatus Status { get; set; }
    public required MaintenanceType Type { get; set; }

    // Navigation properties
    public required EquipmentSystem EquipmentSystem { get; set; }
    public required Technician Technician { get; set; }
    public required ICollection<PlanTask> PlanTasks { get; set; } = new List<PlanTask>();
    public required ICollection<TaskExecution> TaskExecutions { get; set; } = new List<TaskExecution>();
}