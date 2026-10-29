namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class MaintenancePlan : Entity
{
    public required Guid LineId { get; set; }
    public required Guid TechnicianCreatorId { get; set; }
    public required DateOnly CreationDate { get; set; }
    public required PlanStatus Status { get; set; }
    public required bool AllCriteriaMet { get; set; }

    // Navigation properties
    public required ProductionLine Line { get; set; }
    public required Technician Technician { get; set; }
    public required PlanParameters? PlanParameters { get; set; }
    public required ICollection<PlanTask> PlanTasks { get; set; } = new List<PlanTask>();
    public required ICollection<PlanValidation> PlanValidations { get; set; } = new List<PlanValidation>();
    public required ICollection<PlanAssignment> PlanAssignments { get; set; } = new List<PlanAssignment>();
}