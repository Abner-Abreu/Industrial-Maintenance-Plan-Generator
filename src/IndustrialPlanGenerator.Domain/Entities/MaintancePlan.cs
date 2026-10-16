namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class MaintenancePlan: Entity
{
    public required Guid LineId {get;set;}
    public required Guid TechnicianCreatorId {get;set;}
    public required DateOnly CreationDate {get;set;}
    public required PlanStatus Status {get;set;}
    public required bool AllCriteriaMet {get;set;}
}