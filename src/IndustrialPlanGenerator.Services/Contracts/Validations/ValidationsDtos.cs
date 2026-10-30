using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
namespace IndustrialPlanGenerator.Services.Contracts.Validations;


// Validation-1

/// <summary>
/// Datos para la creacion de validaciones
/// </summary>
/// <param name="Result"> Resultado de la validacion (Approved, Rejected)</param>
/// <param name="Comments"> Observaciones respecto al plan</param>
/// <param name="NewPlanRequest"> solicita la creacion de un nuevo plan</param>
public sealed record ValidCreateDto(ValidationResult Result, string? Comments, bool NewPlanRequest);

/// <summary>
/// Datos del jefe
/// </summary>
/// <param name="id"> id del jefe</param>
/// <param name="name"> nombre del jefe</param>
public sealed record PlanManager(Guid id, string name);

/// <summary>
/// Datos de una validacion
/// </summary>
/// <param name="id"> id de la validacion</param>
/// <param name="planId"> id del plan</param>
/// <param name="boss"> datos del jefe</param>
/// <param name="validationDate"> fecha de la validacion</param>
/// <param name="comments"> Observaciones</param>
/// <param name="result"> Resultado de la validacion</param>
public sealed record ValidationDto(Guid id, Guid planId, PlanManager boss, DateOnly validationDate, string comments, ValidationResult result);




// Validation-2

/// <summary>
/// Filtro para obtener validaciones de acuerdo a los parametros
/// </summary>
/// <param name="ReviewerId"> Id del Revisor</param>
/// <param name="Result"> Resultado de la validacion (Approved, Rejected)</param>
/// <param name="Date"> Fecha de la validacion</param>
public sealed record ValidFilterDto(Guid? ReviewerId, string? Result, DateTime? Date);