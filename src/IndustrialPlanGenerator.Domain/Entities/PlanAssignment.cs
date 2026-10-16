namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class TaskAssignment: Entity
{
    public required Guid PlanId {get;set;}
    public required Guid OperatorId {get;set;}
    public required DateOnly AssignmentDate {get;set;}
}