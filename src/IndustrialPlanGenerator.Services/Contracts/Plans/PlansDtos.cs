using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
namespace IndustrialPlanGenerator.Services.Contracts.Plans;


// Plans-1

/// <summary>
/// Datos de los parametros del plan
/// </summary>
/// <param name="PreventiveProportion"> datos de proporcion preventiva</param>
/// <param name="CorrectiveProportion"> datos de proporcion correctiva</param>
/// <param name="PredictiveProportion"> datos de proporcion predictiva</param>
/// <param name="TotalTasks"> datos de las tareas totales</param>
/// <param name="SystemCoverage"> datos de la covertura del sistema</param>
public sealed record PlanParametersDto(double PreventiveProportion, double CorrectiveProportion, double PredictiveProportion, int TotalTasks, double SystemCoverage);

/// <summary>
/// Dto de salida para obtener informacion de una tarea
/// </summary>
/// <param name="id"> id de la tarea</param>
/// <param name="description"> descripcion de la tarea</param>
/// <param name="criticality"> criticidad de la tarea</param>
/// <param name="systemId"> id del sistema o equipo al que responde la tarea</param>
public sealed record PlanTaskDto(Guid id, string description, Criticality criticality, Guid systemId);

/// <summary>
/// Previsualizacion de tareas y parametros para la posterior creacion de un plan
/// </summary>
/// <param name="LineId"> Identificador de la linea</param>
/// <param name="Parameters"> Parametros del plan (Proporcion de tipo, Cobertura, Cantidad, Criticidad Minina)</param>
/// <param name="Tasks"> Tareas que componen el plan</param>
/// <param name="Strategy"> Estrategia a seguir</param>
public sealed record PlanPrevDto(Guid LineId, PlanParametersDto Parameters, List<PlanTaskDto> Tasks, string Strategy);



/// <summary>
/// Dto para guardar la informacion de los tecnicos
/// </summary>
/// <param name="id"> id del tecnico</param>
/// <param name="name"> nombre del tecnico</param>
public sealed record PlanTechDto(Guid id, string name);

/// <summary>
/// Dto de Salida para la creacion de un plan
/// </summary>
/// <param name="id"> id del plan</param>
/// <param name="lineId"> id de la linea del plan</param>
/// <param name="tech"> datos del tecnico que creo el plan</param>
/// <param name="creationDate"> fecha de creacion</param>
/// <param name="Status"> estatus del plan</param>
/// <param name="AllCriteriaMet"> Booleano de criterios cumplidos</param>
public sealed record PlanDto(Guid id, Guid lineId, PlanTechDto tech, DateOnly creationDate, PlanStatus Status, bool AllCriteriaMet);





// Plans-3

/// <summary>
/// Obtener Planes con los valores de filtrado
/// </summary>
/// <param name="LineId"> Identificador de la Linea (opcional)</param>
/// <param name="Status"> Estado del Plan (Generated, Validated, Rejected) (opcional)</param>
/// <param name="CreatorId"> Identificador del Creador (opcional)</param>
/// <param name="Start"> Fecha de Creacion (opcional)</param>
/// <param name="Finish"> Fecha de Finalizacion (opcional)</param>
public sealed record PlanFilterDto(Guid? LineId, PlanStatus? Status, Guid? CreatorId, DateOnly? Start, DateOnly? Finish);





// Plans-4

/// <summary>
/// Dto para guardar la informacion de el jefe
/// </summary>
/// <param name="id"> id del tecnico</param>
/// <param name="name"> nombre del tecnico</param>
public sealed record PlanManagerDto(Guid id, string name);

/// <summary>
/// Informacion de la validacion de un plan
/// </summary>
/// <param name="id"> id de la validacion</param>
/// <param name="boss"> informacion del jefe</param>
/// <param name="validacionDate"> fecha de validacion</param>
/// <param name="comments"> Observaciones</param>
/// <param name="result"> resultado de validacion</param>
public sealed record PlanValidDto(Guid id, PlanManagerDto boss, DateOnly validacionDate, string comments, ValidationResult result);

/// <summary>
/// Dto de Salida para obtener un plan con parametros y validacion
/// </summary>
/// <param name="Plan"> Plan de Mantenimiento</param>
/// <param name="Parameters">  Parametros del plan</param>
/// <param name="Tasks"> Tareas del plan</param>
/// <param name="Validation"> Validacion del plan</param>
public sealed record PlanViewDto( PlanDto Plan, PlanParametersDto Parameters, List<PlanTask> Tasks, PlanValidDto Validation);





// Plans-5

/// <summary>
/// Dto para guardar la informacion de el jefe
/// </summary>
/// <param name="id"> id del tecnico</param>
/// <param name="name"> nombre del tecnico</param>
public sealed record PlanOperators(Guid id, string name);

/// <summary>
/// Informacion de las asignaciones
/// </summary>
/// <param name="id"> id de las asignaciones</param>
/// <param name="operators"> informacion de los operadores</param>
public sealed record PlanAssigDto(Guid id, PlanOperators operators);

/// <summary>
/// Informacion sobre la ejecucion de una tarea de un plan
/// </summary>
/// <param name="id"> id de la ejecucion</param>
/// <param name="task"> informacion de la tarea</param>
/// <param name="date"> fecha de la informacion</param>
/// <param name="compliance"> booleano de cumplimiento</param>
/// <param name="comments"> observaciones</param>
public sealed record PlanExecDto(Guid id, PlanTask task, DateOnly date, bool compliance, string comments);

/// <summary>
/// Informacion de una reinspeccion
/// </summary>
/// <param name="id"> id de la reinspeccion</param>
/// <param name="execId"> id de la ejecucion</param>
/// <param name="tech"> informacion del tecnico</param>
/// <param name="Status"> estado de la reinspeccion</param>
/// <param name="ManualResult"> resultado manual de la reinspeccion</param>
public sealed record PlanReinspDto(Guid id, Guid execId, PlanTechDto tech, ReinspectionStatus Status, string? ManualResult);


/// <summary>
/// Dto de salida con funcion de historial de un plan especifico por URL->id
/// </summary>
/// <param name="Validation"> Validacion del plan objetivo</param>
/// <param name="Operators"> Operadores que son asignados a dicho plan</param>
/// <param name="Executions"> Ejecucion de Tareas de dichos Operadores</param>
/// <param name="Reinspections"> Solicitudes de Reinspeccion sobre dicho plan</param>
public sealed record PlanTrazabilityDto( PlanValidDto Validation, List<PlanAssigDto> Assigments, List<PlanExecDto> Executions, List<PlanReinspDto> Reinspections);