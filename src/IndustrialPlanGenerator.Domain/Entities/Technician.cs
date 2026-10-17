namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class Technician : Entity
{
    public required string Specialty { get; set; }
    

    //Navigation properties only
    public ICollection<TechnicianLine> Lines {get;set;} = new List<TechnicianLine>();
    public ICollection<MaintenancePlan> MaintenancePlans {get;set;} = new List<MaintenancePlan>();
    public ICollection<MaintenanceTask> MaintenanceTasks {get;set;} = new List<MaintenanceTask>();
    public ICollection<ReinspectionRequest> ReinspectionRequests {get;set;} = new List<ReinspectionRequest>();
}