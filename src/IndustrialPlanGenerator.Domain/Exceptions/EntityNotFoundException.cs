namespace IndustrialPlanGenerator.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public Type EntityType { get; }
    public object Id { get; }

    public EntityNotFoundException(Type entityType, object id)
        : base($"{entityType.Name} with id '{id}' was not found.")
    {
        EntityType = entityType;
        Id = id;
    }

    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.")
    {
        EntityType = null!;
        Id = id;
    }
}