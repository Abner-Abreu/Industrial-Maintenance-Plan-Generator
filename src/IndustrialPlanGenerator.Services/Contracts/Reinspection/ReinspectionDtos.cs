using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
namespace IndustrialPlanGenerator.Services.Contracts.Reinspection;


// Reinspection-1

/// <summary>
/// Dto para guardar datos de una ejecucion para mostrar en la reinspeccion
/// </summary>
/// <param name="executionId"></param>
/// <param name="taskId"></param>
/// <param name="comments"></param>
public sealed record ReinspectionExecDto(Guid executionId, Guid taskId, string comments);

/// <summary>
/// Dto para guardar la informacion de los tecnicos
/// </summary>
/// <param name="id"> id del tecnico</param>
/// <param name="name"> nombre del tecnico</param>
public sealed record ReinspectionTechDto(Guid id, string name);

/// <summary>
/// Creacion de Reinspecciones
/// </summary>
/// <param name="execution"> datos de la ejecucion a reinspeccionar</param>
/// <param name="tech"> Tecnico que supervisa la reinspeccion (opcional)</param>
public sealed record ReinspectionCreateDto(ReinspectionExecDto execution, ReinspectionTechDto? tech);

/// <summary>
/// Dto de salida con los datos de una reinspeccion
/// </summary>
/// <param name="id"> id de la reinspeccion</param>
/// <param name="exec"> datos de la ejecucion</param>
/// <param name="tech"> datos del tecnico</param>
/// <param name="date"> fecha de la reinspeccion</param>
/// <param name="status"> estatus de la reinspeccion</param>
/// <param name="manualResult"> resultado manual de la reinspeccion</param>
public sealed record ReinspectionDto(Guid id, ReinspectionExecDto exec, ReinspectionTechDto? tech, DateOnly date, ReinspectionStatus status, string? manualResult);





// Reinspection-2

/// <summary>
/// Obtener Reinspecciones por filtros
/// </summary>
/// <param name="Status"> Estado de la reinspeccion (Pending, InProgress, Completed)</param>
/// <param name="LineId"> Identificador de la Linea de la reinspeccion</param>
/// <param name="OperatorId"> Identificador del operador</param>
/// <param name="TechId"> Identificador del tecnico</param>
public sealed record ReinspectionFilterDto(ReinspectionStatus? Status, Guid? LineId, Guid? OperatorId, Guid? TechId);

/// <summary>
/// Dto de salida para obtener una Lista de reinspecciones
/// </summary>
/// <param name="Reinspections"> Datos de las reinspecciones</param>
public sealed record ReinspectionListDto(List<ReinspectionDto> Reinspections);





// Reinspection-4

/// <summary>
/// Resultado de la reinspeccion
/// </summary>
/// <param name="ManualResult"> Informacion del resultado</param>
public sealed record ReinspectionResultDto(string? ManualResult);