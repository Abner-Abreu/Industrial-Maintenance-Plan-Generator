namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Shift : Entity
{
    public required string Name { get; set; }

    public required string Schedule { get; set; }

    //Navigation properties only
    public ICollection<Operator> Operators {set;get;} = new List<Operator>();
}