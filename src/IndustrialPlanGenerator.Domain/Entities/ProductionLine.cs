namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class ProductionLine : Entity
{
    public required string Name { get; set; }

    public required string ProductiveProcess { get; set; }

    //Navigation properties only
    public ICollection<EquipmentSystem> EquipmentSystems {get;set;} = new List<EquipmentSystem>();
    public ICollection<TechnicianLine> TechnicianLines {get;set;} = new List<TechnicianLine>();
    public ICollection<Operator> Operators {get;set;} = new List<Operator>();
    public ICollection<MaintenancePlan> MaintenancePlans {get;set;} = new List<MaintenancePlan>();
}
