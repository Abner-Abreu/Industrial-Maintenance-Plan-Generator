namespace IndustrialPlanGenerator.Domain.Services;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.ValueObjects;

public interface ITaskPlanGenerator
{
    //RF-09
    GeneratedPlan Generate(
        IReadOnlyCollection<MaintenanceTask> availableTasks,
        IReadOnlyCollection<EquipmentSystem> systemsOnline,
        PlanGenerationParameters parameters,
        Guid lineId,
        Guid technicianCreatorId,
        CancellationToken ct = default);
}