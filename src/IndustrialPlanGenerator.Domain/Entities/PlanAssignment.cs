namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class PlanAssignment : Entity
{
    public required Guid PlanId { get; set; }
    public required Guid OperatorId { get; set; }
    public required DateOnly AssignmentDate { get; set; }

    // Navigation properties
    public required MaintenancePlan MaintenancePlan { get; set; }
    public required Operator Operator { get; set; }
    public required ICollection<TaskExecution> TaskExecutions { get; set; } = new List<TaskExecution>();
}