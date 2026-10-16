namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class ReinspectionRequest: Entity
{
    public required Guid ExecutionId {get;set;}
    public required Guid OperatorId {get;set;}
    public Guid? SupportTechnicianId {get;set;}
    public required DateOnly RequestDate {get;set;}
    public required ReinspeccionStatus Status {get;set;}
    public string? ManualResult {get;set;}
    public DateOnly? ResultDate {get;set;}
}