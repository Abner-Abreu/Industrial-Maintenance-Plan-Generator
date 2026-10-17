namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class PlanAssignment: Entity
{
    public required Guid PlanId {get;set;}
    public required Guid OperatorId {get;set;}

    //Navigation properties only
    public MaintenancePlan MaintenancePlan {get;set;} = null!;
    public Operator Operator {get;set;} = null!;
    public ICollection<TaskExecution> TaskExecutions {get;set;} = new List<TaskExecution>();
}