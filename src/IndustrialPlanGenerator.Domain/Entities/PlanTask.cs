namespace IndustrialPlanGenerator.Domain.Entities;

public class PlanTask
{
    public required Guid PlanId { get; set; }
    public required Guid TaskId { get; set; }

    public required int Order { get; set; }

    // Navigation properties
    public required MaintenancePlan MaintenancePlan { get; set; }
    public required MaintenanceTask MaintenanceTask { get; set; }
}