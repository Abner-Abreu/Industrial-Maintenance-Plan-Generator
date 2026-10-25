namespace IndustrialPlanGenerator.Domain.ValueObjects;

using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;

public sealed record PlanGenerationParameters
{
    public double PreventiveProportion {get;}
    public double CorrectiveProportion {get;}
    public double PredictiveProportion {get;}
    public double SystemCoverage {get;}
    public int TotalTasks {get;}
    public Criticality MinCriticality{get;}

    public PlanGenerationParameters(
        double preventiveProportion,
        double correctiveProportion,
        double predictiveProportion,
        double systemCoverage,
        int totalTasks,
        Criticality minCriticality)
    {
        if (preventiveProportion is < 0 or > 1 ||
            correctiveProportion is < 0 or > 1 ||
            predictiveProportion is < 0 or > 1)
            throw new DomainException("Each proportion most be in [0,1].");

        if (Math.Abs(preventiveProportion + correctiveProportion + predictiveProportion - 1.0) > 1e-6)
            throw new DomainException("All proportions most add 1.");

        if (systemCoverage is < 0 or > 1)
            throw new DomainException("System coverage most be in [0,1].");

        if (totalTasks <= 0)
            throw new DomainException("Total task most be a positive value.");

        PreventiveProportion = preventiveProportion;
        CorrectiveProportion = correctiveProportion;
        PredictiveProportion = predictiveProportion;
        SystemCoverage = systemCoverage;
        TotalTasks = totalTasks;
        MinCriticality = minCriticality;
    }
}