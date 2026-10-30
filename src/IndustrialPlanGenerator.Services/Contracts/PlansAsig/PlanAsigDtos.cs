using IndustrialPlanGenerator.Domain.Entities;
namespace IndustrialPlanGenerator.Services.Contracts.PlanAssig;


//PlanAssigment-1

/// <summary>
/// Asignacion de operarios a los planes
/// </summary>
/// <param name="PlanId"> Identificador del plan</param>
/// <param name="OperatorId"> Identificador del operador</param>
public sealed record AssignationCreateDto(Guid PlanId, Guid OperatorId);

/// <summary>
/// Dto de salida para obtener asignaciones creadas
/// </summary>
/// <param name="id"> id de la asignacion</param>
/// <param name="PlanId"> id del plan</param>
/// <param name="OperatorId"> id del operador</param>
/// <param name="date"> fecha de creacion de la asignacion</param>
public sealed record AssigGetOne(Guid id, Guid PlanId, Guid OperatorId, DateOnly date);





//PlanAssigment-3

/// <summary>
/// Obtener Asignaciones de los operarios a los planes de acuerdo a los filtros
/// </summary>
/// <param name="OperatorId"> Identificador del Operador</param>
/// <param name="LineId"> Identificador de la Linea</param>
/// <param name="Start"> Fecha de Asignacion</param>
/// <param name="Finish"> Fecha de Finalizacion del Plan</param>
public sealed record AssignationFilterDto(Guid? OperatorId, Guid? LineId, DateOnly? Start, DateOnly? Finish);