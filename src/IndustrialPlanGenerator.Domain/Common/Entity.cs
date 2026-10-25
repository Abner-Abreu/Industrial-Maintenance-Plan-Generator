namespace IndustrialPlanGenerator.Domain.Common;

public abstract class Entity
{
    public Guid Id {get; private init;} = Guid.CreateVersion7();
}
