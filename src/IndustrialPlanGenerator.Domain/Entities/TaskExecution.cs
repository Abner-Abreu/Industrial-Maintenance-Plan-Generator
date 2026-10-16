namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class TaskExecution: Entity
{
    public required Guid TaskId {get;set;}
    public required Guid AssignmentId {get;set;}
    public required DateOnly Date {get;set;}
    public required string Observations {get;set;}
    public required bool Completed {get;set;}
}