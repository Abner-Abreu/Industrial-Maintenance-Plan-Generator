using System.Data.Common;
using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
namespace IndustrialPlanGenerator.Services.Contracts.Reports;


// Endpont Report-1

/// <summary>
/// Dto para guardar la informacion del plan que se desea mostrar en el reporte
/// </summary>
/// <param name="planId"></param>
/// <param name="creatorId"></param>
/// <param name="creatorName"></param>
/// <param name="creationDate"></param>
public sealed record ReportPlanDto(Guid planId, Guid creatorId, string creatorName, DateOnly creationDate);

/// <summary>
/// Dto para guardar la informacion de los parametros que se desea mostrar en el reporte
/// </summary>
/// <param name="preventiveProportion"> parametro PreventiveProportion</param>
/// <param name="correctiveProportion"> parametro CorrectiveProportion</param>
/// <param name="predictiveProportion"> parametro PredictiveProportion</param>
public sealed record ReportParametersDto(double preventiveProportion, double correctiveProportion, double predictiveProportion);

/// <summary>
/// Dto de salida para obtener planes de mantenimiento y sus parametros generados
/// </summary>
/// <param name="Plan"> Datos del Plan de Mantenimiento</param>
/// <param name="Parameter"> Datos de los Parametros</param>
public sealed record ReportPlanParametersDto( ReportPlanDto Plan, ReportParametersDto Parameter);





// Endpont Report-2

/// <summary>
/// Dto de salida para obtener informacion de una tarea que se desea mostrar en el reporte
/// </summary>
/// <param name="id"> id de la tarea</param>
/// <param name="description"> descripcion de la tarea</param>
/// <param name="criticality"> criticidad de la tarea</param>
/// <param name="systemId"> id del sistema o equipo al que responde la tarea</param>
public sealed record ReportTasksDto(Guid id, string description, Criticality criticality, Guid systemId);





// Endpont Report-3

/// <summary>
/// Dto para guardar la informacion de las validaciones que se desea mostrar en el reporte
/// </summary>
/// <param name="validationId"> id de la validacion</param>
/// <param name="result"> resultado de la validacion</param>
/// <param name="date"> fecha de creacion de la validacion</param>
/// <param name="comments"> observaciones de la validacion</param>
public sealed record ReportValidationDto(Guid validationId, ValidationResult result, DateOnly date, string comments);

/// <summary>
/// Dto de salida para obtener planes de mantenimiento generados y su validacion
/// </summary>
/// <param name="Plan"> Plane validado</param>
/// <param name="Validation"> Validacion del plan</param>
public sealed record ReportPlanValidationDto(ReportPlanDto Plan, ReportValidationDto Validation);





// Endpont Report-4

/// <summary>
/// Dto para guardar la informacion de los operadores que se desea mostrar en el reporte
/// </summary>
/// <param name="id"> id del operador</param>
/// <param name="name"> nombre del operador</param>
public sealed record ReportOperatorDto(Guid id, string name);

/// <summary>
/// Obtener el rendimiento de un operador en una tarea que pertenece al plan
/// </summary>
/// <param name="taskId"> id de la tarea</param>
/// <param name="criticality"> criticidad de la tarea</param>
/// <param name="compliance"> cumplimiento de la tarea</param>
public sealed record ReportPerformanceDto(Guid taskId, Criticality criticality, bool compliance);

/// <summary>
/// Rendimiento de los operadores en un plan determinado
/// </summary>
/// <param name="op"> Operador </param>
/// <param name="performance"> Desempeno total sobre un plan</param>
public sealed record ReportOperatorPerformanceDto(ReportOperatorDto op, List<ReportPerformanceDto> performance);





// Endpont Report-5

/// <summary>
/// Distribucion de los sistemas y equipo de una tarea de un plan
/// </summary>
/// <param name="systemId"> id del sistema</param>
/// <param name="systemName"> nombre del sistema</param>
/// <param name="criticality"> criticidad de la tarea</param>
public sealed record ReportDistribDto(Guid systemId, string systemName, Criticality criticality);

/// <summary>
/// Datos de comparacion de un plan de mantenimiento
/// </summary>
/// <param name="distribution"> distribucion por sistemas y criterios</param>
/// <param name="criteriaMet"></param>
public sealed record ReportPlanCompDto(List<ReportDistribDto> distribution, bool criteriaMet);

/// <summary>
/// Valores de comparacion de todos los planes que pertenecen a una linea
/// </summary>
/// <param name="LineasId"> Identificadores de las lineas</param>
/// <param name="Planes"> Lista de resultados de los planes de Mantenimientos (cada plan con sus criterios y distribuciones)</param>
public sealed record ReportComparativaPlanDto(Guid lineId, List<ReportPlanCompDto> comparativeValues);





// Endpont Report-6

/// <summary>
/// Obtener el rendimiento de una tarea perteneciente a un plan de mantenimiento
/// </summary>
/// <param name="task"> Datos de la tarea</param>
/// <param name="compliance"> booleano del cumplimiento de la tarea</param>
/// <param name="comments"> Observaciones sobre el cumplimiento de la tarea</param>
public sealed record ReportAveragePerformanceDto(ReportTasksDto task, bool compliance, string comments);

/// <summary>
/// Obtener la correlacion entre criticidad y el rendimiento promedio en una linea
/// </summary>
/// <param name="criticality"> criticidad objetiva</param>
/// <param name="Performance"> Lista de rendimiento de las tareas que pertenecen a los planes de la linea objetivo</param>
public sealed record ReportLineAvgPerfDto(Criticality criticality, List<ReportAveragePerformanceDto> Performance);





// Endpont Report-7

/// <summary>
/// Dto para guardar la informacion de los tecnicos que se desea mostrar en el reporte
/// </summary>
/// <param name="id"> id del tecnico</param>
/// <param name="Name"> nombre del tecnico</param>
public sealed record ReportTechDto(Guid id, string Name);

/// <summary>
/// Dto para guardar tareas con mayor tasa de incumplimiento
/// </summary>
///<param name="lineId"> id de la linea</param>
/// <param name="task"> informacion de la tarea</param>
/// <param name="tech"> tecnico que creo la tarea</param>
public sealed record ReportNonComplianceTasksDto(Guid lineId, ReportTasksDto task, ReportTechDto tech);





// Endpont Report-8

/// <summary>
/// Obtener comparacion entre los operadores que hicieron una solicitud de reinspeccion contra el total de operadores que trabajan en ese mismo turno y linea
/// </summary>
/// <param name="reinspectionOperators"> Operadores que pidieron reinspeccion</param>
/// <param name="otherOperators"> los operadores del mismo turno que ___NO___ lo hicieron</param>
/// <param name="percentage"> Proporcion de los que pidieron reinspeccion sobre el total</param>
public sealed record ReportReinspectionVsTurnDto(List<ReportOperatorDto> reinspectionOperators, List<ReportOperatorDto> otherOperators, double percentage);