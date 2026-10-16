namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class ProductionLine : Entity
{
    public required string Name { get; set; }

    public required string ProductiveProcess { get; set; }
}
