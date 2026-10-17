namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class PlanParameters: Entity
{
    public required Guid PlanId {get;set;}
    public required MaintenanceType Type {get;set;}
    public required int TotalTasks {get;set;}
    public required string SystemCoverage {get;set;}

    //Navigation properties only
    public MaintenancePlan MaintenancePlan {get;set;} = null!;
}