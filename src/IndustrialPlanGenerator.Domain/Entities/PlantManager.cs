namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class PlantManager : Entity, ISoftDeletable
{
    public bool IsDeleted { get; set; }

    // Navigation properties
    public required ICollection<PlanValidation> PlanValidations { get; set; } = new List<PlanValidation>();
}