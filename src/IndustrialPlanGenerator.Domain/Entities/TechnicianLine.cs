namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Enums;

public class TechnicianLine
{
    public required Guid TechnicianId {get;set;}
    public required Guid LineId {get;set;}
    public required TechnicianPermission Permission {get;set;}
}