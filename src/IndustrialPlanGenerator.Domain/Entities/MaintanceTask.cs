namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class MaintenanceTask : Entity
{
    public required string Description {get;set;}
    public required Guid SystemId {get;set;}
    public required Criticality Criticality {get;set;}
    public required Guid ThechnicianCreatorId {get;set;}
    public required DateOnly CreationDate {get;set;}
    public required TaskStatus Status {get;set;}

    //Navigation properties only

    public EquipmentSystem EquipmentSystem {get;set;} = null!;
    public Technician Technician {get;set;} = null!;
    public ICollection<PlanTask> PlanTasks {get;set;} = new List<PlanTask>();
    public ICollection<TaskExecution> TaskExecutions {get;set;} = new List<TaskExecution>();
}