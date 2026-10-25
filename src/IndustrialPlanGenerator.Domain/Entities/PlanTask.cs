namespace IndustrialPlanGenerator.Domain.Entities;

public class PlanTask
{
    public required Guid PlanId {get;set;}
    public required Guid TaskId {get;set;}

    public required int Order {get;set;}
    
    //Navigation properties only
    public MaintenancePlan MaintenancePlan {get;set;} = null!;
    public MaintenanceTask MaintenanceTask {get;set;} = null!;
}