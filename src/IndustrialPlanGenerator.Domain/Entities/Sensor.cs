namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class Sensor : Entity
{
    public required Guid EquipmentId { get; set; }
    public required SensorType Type { get; set; }

    // Navigation properties
    public required EquipmentSystem Equipment { get; set; }
}