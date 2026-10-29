namespace IndustrialPlanGenerator.Domain.Common;

public abstract class Entity
{
    public required Guid Id { get; init; } = Guid.CreateVersion7();

    public override bool Equals(object? obj)
        => obj is Entity other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? a, Entity? b) => a?.Id == b?.Id;
    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}