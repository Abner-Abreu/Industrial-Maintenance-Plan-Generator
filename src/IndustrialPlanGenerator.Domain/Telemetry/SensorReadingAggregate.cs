namespace IndustrialPlanGenerator.Domain.Telemetry;

using IndustrialPlanGenerator.Domain.Enums;

public sealed class SensorReadingAggregate
{
    public Guid? EquipmentId { get; set; }
    public Guid? LineId { get; set; }
    public SensorType Type { get; set; }
    public DateTime Bucket { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public long Count { get; set; }
}