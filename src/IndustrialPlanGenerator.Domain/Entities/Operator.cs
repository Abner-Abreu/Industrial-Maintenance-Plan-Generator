namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Operator : Entity
{
    public required int Age { get; set; }

    public required Guid ShiftId { get; set; }

    //Navigation properties only

    public Shift Shift {set;get;} = null!;
    public ICollection<ProductionLine> Lines {set;get;} = new List<ProductionLine>();
    public ICollection<PlanAssignment> Plans {set;get;} = new List<PlanAssignment>();
    public ICollection<ReinspectionRequest> ReinspectionRequests {set;get;} = new List<ReinspectionRequest>();
}