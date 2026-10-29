namespace IndustrialPlanGenerator.Domain.Entities;

public class OperatorLine
{
    public required Guid OperatorId { get; set; }
    public required Guid LineId { get; set; }

    // Navigation properties
    public required Operator Operator { get; set; }
    public required ProductionLine Line { get; set; }
}