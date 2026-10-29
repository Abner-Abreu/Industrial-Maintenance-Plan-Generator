namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Operator : Entity
{
    public required int Age { get; set; }

    public required Guid ShiftId { get; set; }

    // Navigation properties
    public required Shift Shift { get; set; }
    public required ICollection<OperatorLine> Lines { get; set; } = new List<OperatorLine>();
    public required ICollection<PlanAssignment> Plans { get; set; } = new List<PlanAssignment>();
    public required ICollection<ReinspectionRequest> ReinspectionRequests { get; set; } = new List<ReinspectionRequest>();
}