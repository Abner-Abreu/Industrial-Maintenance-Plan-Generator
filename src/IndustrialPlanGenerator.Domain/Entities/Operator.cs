namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Operator : Entity
{
    public required int Age { get; set; }

    public required Guid ShiftId { get; set; }

    //Navigation properties only

    public Shift Shift {get;set;} = null!;
    public ICollection<ProductionLine> Lines {get;set;} = new List<ProductionLine>();
    public ICollection<PlanAssignment> Plans {get;set;} = new List<PlanAssignment>();
    public ICollection<ReinspectionRequest> ReinspectionRequests {get;set;} = new List<ReinspectionRequest>();
}