using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.PlanAssig;

public interface IAssigPlans
{
    /// <summary>
    /// Crear asignacion
    /// </summary>
    /// <param name="assig"> datos de la asignacion</param>
    /// <returns> Asignacion con datos booleanos de confirmacion (bool IsValid, bool belongline, bool IsDouble)</returns>
    Task<(AssigGetOne, bool, bool, bool)> CreateAssig(AssignationCreateDto assig);


    /// <summary>
    /// Cancelar una asignacion
    /// </summary>
    /// <param name="id">id de la asignacion</param>
    /// <returns> booleano que confirma si se llego a encontrar la asignacion</returns>
    Task<bool> CancelAssig(Guid id);


    /// <summary>
    /// Obtener asignaciones con valores de filtrado
    /// </summary>
    /// <param name="user"> usuario</param>
    /// <param name="filter"> parametros del filtro</param>
    /// <returns> asignaciones que cumplen con el filtrado</returns>
    Task<List<AssigGetOne>> GetAssig(ClaimsPrincipal user, AssignationFilterDto filter);
}