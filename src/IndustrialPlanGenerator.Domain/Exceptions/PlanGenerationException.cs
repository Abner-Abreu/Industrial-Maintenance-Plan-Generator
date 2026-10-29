namespace IndustrialPlanGenerator.Domain.Exceptions;

public class PlanGenerationException : DomainException
{
    public PlanGenerationException() { }

    public PlanGenerationException(string message)
        : base(message) { }

    public PlanGenerationException(string message, Exception innerException)
        : base(message, innerException) { }
}