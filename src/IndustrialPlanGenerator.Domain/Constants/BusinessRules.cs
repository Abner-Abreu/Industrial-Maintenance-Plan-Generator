namespace IndustrialPlanGenerator.Domain.Constants;

public static class BusinessRules
{
    // RI-27
    public const int MinimumOperatorAge = 18;

    // RF-22.
    public const int TopUnfulfilledTasksCount = 10;

    // Logical max length
    public const int MaxTasksPerPlan = 100;

    // RNF-03 / RNF-01 Cache TTL
    public static readonly TimeSpan EquipmentStatusCacheTtl = TimeSpan.FromMinutes(5);

    // Max length for text properties validation
    public const int MaxTaskDescriptionLength = 500;
    public const int MaxObservationLength = 1000;
}