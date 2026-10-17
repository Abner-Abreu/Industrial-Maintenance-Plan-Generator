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

    //Navigation properties only
    public ProductionLine Line {get;set;} = null!;
    public Technician Technician {get;set;} = null!;
    public PlanParameters? PlanParameters {get;set;} = null!;
    public ICollection<PlanTask> PlanTasks {get;set;} = new List<PlanTask>();
    public ICollection<PlanValidation> PlanValidations {get;set;} = new List<PlanValidation>();
    public ICollection<PlanAssignment> PlanAssignments {get;set;} = new List<PlanAssignment>();
}