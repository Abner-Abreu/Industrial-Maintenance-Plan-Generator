namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class PlanValidation : Entity
{
    public required Guid MaintenancePlanId { get; set; }
    public required Guid PlantManagerId { get; set; }
    public required DateOnly ValidationDate { get; set; }
    public required string Observations { get; set; }
    public required ValidationResult Result { get; set; }
    public required bool NewPlanRequired { get; set; }

    // Navigation properties
    public required MaintenancePlan MaintenancePlan { get; set; }
    public required PlantManager PlantManager { get; set; }
}