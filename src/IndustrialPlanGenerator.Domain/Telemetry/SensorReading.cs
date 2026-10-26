namespace IndustrialPlanGenerator.Domain.Telemetry;

using IndustrialPlanGenerator.Domain.Enums;

public sealed class SensorReading
{
    public Guid Id { get; set; }              
    public Guid SensorId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid LineId { get; set; }
    public SensorType Type { get; set; }    
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }    
}