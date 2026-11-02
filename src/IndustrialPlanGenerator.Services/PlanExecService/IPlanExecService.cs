using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.PlanExec;

public interface IPlanExecService
{
    /// <summary>
    /// Obtener el avance de ejecucion de la asignacion del usuario (Operador)
    /// </summary>
    /// <param name="lineid"> id de la linea (opcional)</param>
    /// <param name="user"> usuario</param>
    /// <returns> retornar el avance de la asignacion(ExecProgress)</returns>
    Task<(List<ExecProgressDto> progress, bool notFound)> GetExecutionProgress(Guid? lineid, ClaimsPrincipal user);


    /// <summary>
    /// Obtener el avance de ejecucion por id de la asignacion
    /// </summary>
    /// <param name="id"> id de la asignacion</param>
    /// <param name="user"> usuario</param>
    /// <returns> retornar el avance de la asignacion, o null en caso de no encontrarla</returns>
    Task<(ExecProgressbyAssig progress, bool notFound)> GetExecutionbyId(Guid? id, ClaimsPrincipal user);


    /// <summary>
    /// Registrar la ejecucion de una tarea
    /// </summary>
    /// <param name="taskid"> id de la tarea de mantenimiento</param>
    /// <param name="execinfo"> informacion sobre la ejecucion</param>
    /// <returns> Booleanos de verificacion: NotAssig->Asignacion no activa, NoTaskPlan->Tarea no pertenece al plan, IsRegistered:Si la tarea esta registrada</returns>
    Task<(PlanExecutionsDto execution, bool NotAssig, bool NoTaskPlan, bool IsRegistered)> CreateExecution(Guid taskid, ExecCreateDto execinfo);

}