namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class ProductionLine : Entity
{
    public required string Name { get; set; }

    public required string ProductiveProcess { get; set; }

    // Navigation properties
    public required ICollection<EquipmentSystem> EquipmentSystems { get; set; } = new List<EquipmentSystem>();
    public required ICollection<TechnicianLine> TechnicianLines { get; set; } = new List<TechnicianLine>();
    public required ICollection<OperatorLine> Operators { get; set; } = new List<OperatorLine>();
    public required ICollection<MaintenancePlan> MaintenancePlans { get; set; } = new List<MaintenancePlan>();
}
