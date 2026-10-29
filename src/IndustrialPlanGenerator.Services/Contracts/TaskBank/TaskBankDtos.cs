using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using TaskStatus = IndustrialPlanGenerator.Domain.Entities.TaskStatus;
namespace IndustrialPlanGenerator.Services.Contracts.TaskBank;


// Tasks Bank-1

/// <summary>
/// Obtener Tareas con los valores de filtrado
/// </summary>
/// <param name="LineId"> Identificador de Linea (opcional)</param>
/// <param name="SystemId"> Identificador del Sistema o Equipo (opcional)</param>
/// <param name="Type"> Tipo (Preventivo, Correctivo, Predictivo) (opcional)</param>
/// <param name="Criticality"> Criticidad (Bajo, Medio, Alto) (opcional)</param>
/// <param name="Status"> Estado (Activo, Duplicado) (opcional)</param>
/// <param name="CreatorId"> Identificador del Creador (opcional)</param>
/// <param name="Date"> Fecha de Creacion (opcional)</param>
/// <param name="Description"> Descripcion de la Tarea (opcional)</param>
public sealed record TaskFilterDto(Guid? LineId, Guid? SystemId, MaintenanceType? Type, Criticality? Criticality, TaskStatus? Status, Guid? CreatorId, DateOnly? Date, string? Description);

/// <summary>
/// Dto de salida para obtener informacion de una tarea
/// </summary>
/// <param name="id"> id de la tarea</param>
/// <param name="description"> descripcion de la tarea</param>
/// <param name="criticality"> criticidad de la tarea</param>
/// <param name="systemId"> id del sistema o equipo al que responde la tarea</param>
public sealed record TaskDto(Guid id, string description, Criticality criticality, Guid systemId);





// Tasks Bank-3

/// <summary>
/// Datos para la creacion de tareas
/// </summary>
/// <param name="Description"> Descripcion de la tarea</param>
/// <param name="SystemId"> Identificador del Sistema o Equipo</param>
/// <param name="Type"> Tipo (Preventivo, Correctivo, Predictivo)</param>
/// <param name="Criticality"> Criticidad (Bajo, Medio, Alto)</param>
public sealed record TaskCreateDto(string Description, Guid SystemId, MaintenanceType Type, Criticality Criticality);





// Tasks Bank-4

/// <summary>
/// Reclasificar la Tarea cambiando los parametros
/// </summary>
/// <param name="SystemId"> Identificador del Sistema</param>
/// <param name="Type"> Tipo (Preventivo, Correctivo, Predictivo)</param>
/// <param name="Criticality"> Criticidad (Bajo, Medio, Alto)</param>
public sealed record TaskUpdateDto(Guid SystemId, MaintenanceType Type, Criticality Criticality);