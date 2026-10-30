using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
namespace IndustrialPlanGenerator.Services.Contracts.PlanExec;


// PlanExecution-1

/// <summary>
/// Informacion de una asignacion
/// </summary>
/// <param name="id"> id de la asignacion</param>
/// <param name="planId"> id del plan asociado</param>
/// <param name="date"> fecha de la asignacion</param>
public sealed record MyAssigDto(Guid id, Guid planId, DateOnly date);

/// <summary>
/// Dto de salida para obtener informacion de una tarea
/// </summary>
/// <param name="id"> id de la tarea</param>
/// <param name="description"> descripcion de la tarea</param>
/// <param name="criticality"> criticidad de la tarea</param>
/// <param name="systemId"> id del sistema o equipo al que responde la tarea</param>
public sealed record ExecTaskDto(Guid id, string description, Criticality criticality, Guid systemId);

/// <summary>
/// Informacion sobre una ejecucion
/// </summary>
/// <param name="id"> id de la ejecucion</param>
/// <param name="taskId"> id de la tarea en cuestion</param>
/// <param name="compliance"> booleano de cumplimiento</param>
/// <param name="comments"> comentarios de la ejecucion</param>
public sealed record PlanExecutionsDto(Guid id, ExecTaskDto task, bool compliance, string? comments);

/// <summary>
/// Datos para ver las asignaciones de los operadores y el progreso de estas
/// </summary>
/// <param name="Plans"> Planes a la que son asignados (Poseen las tareas)</param>
/// <param name="Execution"> Ejecucion de dichas tareas (Criterios del avance de las tareas)</param>
public sealed record ExecProgressDto(MyAssigDto assignation, List<PlanExecutionsDto> executions);





// PlanExecution-2

/// <summary>
/// Dto de informacion sobre el avance de una asignacion determinada
/// </summary>
/// <param name="executions"> ejecucion de esa asignacion</param>
public sealed record ExecProgressbyAssig(List<PlanExecutionsDto> executions);





// PlanExecution-3

/// <summary>
/// Registro de entrada de ejecucion de tareas
/// </summary>
/// <param name="taskId"> Identificador de la tarea a registrar</param>
/// <param name="fulfilled"> Booleano del cumplimiento de la tarea</param>
/// <param name="comments"> Observaciones de la ejecucion</param>
public sealed record ExecCreateDto(Guid taskId, bool fulfilled, string comments);