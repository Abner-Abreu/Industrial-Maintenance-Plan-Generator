namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;
using IndustrialPlanGenerator.Domain.Enums;

public class User : Entity
{
    public required string Name { get; set; }

    public required UserRole Role { get; set; }

    public required string Credentials { get; set; }
}