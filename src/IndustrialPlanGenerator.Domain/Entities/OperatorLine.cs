namespace IndustrialPlanGenerator.Domain.Entities;

public class OperatorLine
{
    public required Guid OperatorId { get; set; }
    public required Guid LineId { get; set; }

    // Navigation properties only
    public Operator Operator { get; set; } = null!;
    public ProductionLine Line { get; set; } = null!;
}