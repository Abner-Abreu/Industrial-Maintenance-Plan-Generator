namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class Sensor: Entity
{
    public required Guid EquipmentId {get;set;}
    public required SensorType  Type {get;set;}

    //Navigation properties only
    public EquipmentSystem Equipment{get;set;} = null!;
}