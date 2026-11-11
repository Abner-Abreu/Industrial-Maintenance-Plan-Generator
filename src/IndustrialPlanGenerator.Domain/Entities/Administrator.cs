namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Administrator : Entity, ISoftDeletable
{
    public bool IsDeleted { get; set; }
}