namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Operator : Entity
{
    public required int Age { get; set; }

    public required Guid ShiftId { get; set; }
}