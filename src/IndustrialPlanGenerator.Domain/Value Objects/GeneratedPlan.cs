namespace IndustrialPlanGenerator.Domain.ValueObjects;

public sealed record GeneratedPlan(
    IReadOnlyList<SelectedTask> Tasks,
    PlanGenerationParameters Parameters,
    double SystemCoverage,   
    bool AllCriteriaMet);    // RI-32

    
