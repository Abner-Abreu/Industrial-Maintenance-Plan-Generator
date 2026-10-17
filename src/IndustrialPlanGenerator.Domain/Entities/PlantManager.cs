namespace IndustrialPlanGenerator.Domain.Entities;

using IndustrialPlanGenerator.Domain.Common;

public class PlantManager : Entity
{
    //Navigation properties only
    public ICollection<PlanValidation> PlanValidations {get;set;} = new List<PlanValidation>();
    
}