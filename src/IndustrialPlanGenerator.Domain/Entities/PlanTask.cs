namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class PlanTask: Entity
{
    public required Guid PlanId {get;set;}
    public required Guid TaskId {get;set;}
    
    //Navigation properties only
    public MaintenancePlan MaintenancePlan {get;set;} = null!;
    public MaintenanceTask MaintenanceTask {get;set;} = null!;
}