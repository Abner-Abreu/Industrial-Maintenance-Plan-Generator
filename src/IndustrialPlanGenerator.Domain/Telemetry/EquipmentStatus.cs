namespace IndustrialPlanGenerator.Domain.Telemetry;

public sealed class EquipmentStatus
{
    public Guid EquipmentId { get; set; }
    public required string OperatingStatus { get; set; }
    public DateTime LastUpdated { get; set; }
}