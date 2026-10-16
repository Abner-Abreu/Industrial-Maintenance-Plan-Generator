namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class Sensor: Entity
{
    public required Guid Equipment {get;set;}
    public required SensorType  Type {get;set;}
}