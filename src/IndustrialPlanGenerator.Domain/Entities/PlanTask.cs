namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class PlanTask: Entity
{
    public required Guid PlanId {get;set;}
    public required Guid TaskId {get;set;}
}