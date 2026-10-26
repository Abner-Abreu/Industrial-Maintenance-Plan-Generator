namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class EquipmentSystem : Entity
{
    public required string Name { get; set;}
    public required string Description {get;set;}
    public required string OperativeState {get; set;}
    public required Guid LineId { get; set; }

    //Navigation properties only
    public ProductionLine ProductionLine {get;set;} = null!;
    public ICollection<Sensor> Sensors {get;set;} = new List<Sensor>();
    public ICollection<MaintenanceTask> MaintenanceTasks {get;set;} = new List<MaintenanceTask>();
}