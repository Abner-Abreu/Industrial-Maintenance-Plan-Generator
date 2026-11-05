namespace IndustrialPlanGenerator.Domain.Exceptions;

using IndustrialPlanGenerator.Domain.Entities;

public class InvalidPlanStateException : DomainException
{
    public PlanStatus CurrentState { get; }
    public PlanStatus[] AllowedStates { get; }

    public InvalidPlanStateException(PlanStatus current, params PlanStatus[] allowed)
        : base($"Plan is in state '{current}'. Allowed states: {string.Join(", ", allowed)}")
    {
        CurrentState = current;
        AllowedStates = allowed;
    }

    public InvalidPlanStateException(string message)
        : base(message)
    {
        CurrentState = default;
        AllowedStates = [];
    }
}