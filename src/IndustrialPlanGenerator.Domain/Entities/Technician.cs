namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Technician : Entity
{
    public required string Specialty { get; set; }
}