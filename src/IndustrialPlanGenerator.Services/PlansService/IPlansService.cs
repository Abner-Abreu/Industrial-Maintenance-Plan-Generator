using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.Plans;


public interface IPlansService
{
    /// <summary>
    /// Crear un plan de mantenimiento previsualizado
    /// </summary>
    /// <param name="user"> Usuario para verificar linea y permisos de generacion</param>
    /// <param name="planPrev"> Datos para la creacion del plan</param>
    /// <returns> Tareas y criterios del plan</returns>
    Task<PlanDto> CreatePlan(ClaimsPrincipal user, PlanPrevDto planPrev);

    /// <summary>
    /// Crear un plan de mantenimiento que no cumple con los criterios establecidos
    /// </summary>
    /// <param name="user"> Usuario para verificar linea y permisos de generacion</param>
    /// <param name="planPrev"> Datos para la creacion del plan</param>
    /// <returns> Tareas y criterios del plan</returns>
    Task<PlanDto> NonCompliancePlan(ClaimsPrincipal user, PlanPrevDto planPrev);

    /// <summary>
    /// Obtener planes de mantenimiento filtrados por parametros
    /// </summary>
    /// <param name="user"> usuario para obtener linea en caso de tecnico</param>
    /// <param name="filter"> valores a filtrar</param>
    /// <returns> Planes filtrados</returns>
    Task<List<PlanDto>> FilterPlans(ClaimsPrincipal user, PlanFilterDto filter);

    /// <summary>
    /// Ver un plan de mantenimiento
    /// </summary>
    /// <param name="id"> Id del plan</param>
    /// <param name="user"> Usuario para obtener linea en caso de tecnico</param>
    /// <returns> Plan de Mantenimiento con parametros y validaciones, o null si no se encuentra el plan</returns>
    Task<(PlanViewDto result, bool notFound)> ViewPlan(Guid id, ClaimsPrincipal user);

    /// <summary>
    /// Obtener historial de un plan de mantenimiento en especifico
    /// </summary>
    /// <param name="id"> id del plan</param>
    /// <returns> Historial del plan de mantenimiento, o null en caso de no encontrar dicho plan</returns>
    Task<(PlanTrazabilityDto result, bool notFound)> TrazabilityPlan(Guid id);

}