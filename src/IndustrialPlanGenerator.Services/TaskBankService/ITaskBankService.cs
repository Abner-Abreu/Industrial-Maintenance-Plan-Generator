using System.Security.Claims;
namespace IndustrialPlanGenerator.Services.Contracts.TaskBank;

public interface ITaskBankService
{
    /// <summary>
    /// Filtrar Tareas de Mantenimiento
    /// </summary>
    /// <param name="user"> usuario (Necesario para separar logica por rol)</param>
    /// <param name="filter"> Parametros a Filtrar</param>
    /// <returns> Tareas filtradas</returns>
    Task<List<TaskDto>> FilterTasks(ClaimsPrincipal user, TaskFilterDto filter);

    /// <summary>
    /// Obtener Tarea de Mantenimiento con un ID en especifico
    /// </summary>
    /// <param name="user"> usuario (Necesario para separar logica por rol)</param>
    /// <param name="id"> Identificador de la tarea</param>
    /// <returns> Tarea de Mantenimiento con igual id, notFound=true en caso de no encontrarla</returns>
    Task<(TaskDto result, bool notFound)> GetTaskbyId(ClaimsPrincipal user, Guid id);

    /// <summary>
    /// Crear tarea de mantenimiento o retornar una similar
    /// </summary>
    /// <param name="taskinfo"> informacion de la tarea a crear</param>
    /// <returns> Tarea de Mantenimiento nueva. En caso de existir una tarea activa identica activa booleano exist</returns>
    Task<(TaskDto result, bool exist)> CreateTask(TaskCreateDto taskinfo);

    /// <summary>
    /// Permite a un tecnico reclasificar una tarea que le pertenezca
    /// </summary>
    /// <param name="id"> identificador de la tarea</param>
    /// <param name="user"> usuario para validar pertenencia de la tarea</param>
    /// <param name="updates"> Datos de actualizacion de la tarea</param>
    /// <returns> booleano de verificacion: notFound=>tarea no encontrada, invalidplan=>ya esta en plan valido</returns>
    Task<(bool invalidplan, bool notFound)> ReclassifyTask(Guid id, ClaimsPrincipal user, TaskUpdateDto updates);

    /// <summary>
    /// Obtener Tareas Duplicadas de un grupo de tareas activas
    /// </summary>
    /// <returns> Retorna tareas duplicadas</returns>
    Task<List<TaskDto>> GetDuplicatesTasks();

    /// <summary>
    /// Marcar la tarea de mantenimiento como duplicada
    /// </summary>
    /// <param name="id"> id de la tarea</param>
    /// <returns> booleano de verificacion: notFound=>tarea no encontrada, isNotAlreadyDuplicade=>ya estaba como tarea duplicada</returns>
    Task<(bool notFound, bool isAlreadyDuplicade)> MarkDuplicateTasks(Guid id);

    /// <summary>
    /// Desmarcar la tarea de mantenimiento como duplicada
    /// </summary>
    /// <param name="id"> id de la tarea</param>
    /// <returns> booleano de verificacion: notFound=>tarea no encontrada, isNotAlreadyDuplicade=>ya estaba como tarea normal</returns>
    Task<(bool notFound, bool isNotAlreadyDuplicade)> DisMarkDuplicateTasks(Guid id);
}