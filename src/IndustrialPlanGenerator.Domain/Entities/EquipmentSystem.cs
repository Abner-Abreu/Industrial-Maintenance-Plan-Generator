namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class EquipmentSystem : Entity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string OperativeState { get; set; }
    public required Guid LineId { get; set; }

    // Navigation properties
    public required ProductionLine ProductionLine { get; set; }
    public required ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
    public required ICollection<MaintenanceTask> MaintenanceTasks { get; set; } = new List<MaintenanceTask>();
}