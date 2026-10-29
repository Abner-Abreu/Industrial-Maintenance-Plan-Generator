namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Technician : Entity
{
    public required string Specialty { get; set; }

    // Navigation properties
    public required ICollection<TechnicianLine> Lines { get; set; } = new List<TechnicianLine>();
    public required ICollection<MaintenancePlan> MaintenancePlans { get; set; } = new List<MaintenancePlan>();
    public required ICollection<MaintenanceTask> MaintenanceTasks { get; set; } = new List<MaintenanceTask>();
    public required ICollection<ReinspectionRequest> ReinspectionRequests { get; set; } = new List<ReinspectionRequest>();
}