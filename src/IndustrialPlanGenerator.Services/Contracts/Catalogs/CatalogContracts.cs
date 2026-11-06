namespace IndustrialPlanGenerator.Services.Contracts.Catalogs;

/// <summary>Turno de trabajo (CAT-1 a CAT-3).</summary>
/// <param name="Id">Id del turno.</param>
/// <param name="Name">Nombre (único).</param>
/// <param name="Schedule">Horario (único), por ejemplo "06:00-14:00".</param>
public sealed record ShiftDto(Guid Id, string Name, string Schedule);

/// <summary>Datos para crear un turno (CAT-2).</summary>
/// <param name="Name">Nombre (obligatorio y único).</param>
/// <param name="Schedule">Horario (obligatorio y único).</param>
public sealed record CreateShiftRequest(string Name, string Schedule);

/// <summary>Datos para editar un turno (CAT-3). Reemplaza ambos campos.</summary>
/// <param name="Name">Nuevo nombre (obligatorio y único).</param>
/// <param name="Schedule">Nuevo horario (obligatorio y único).</param>
public sealed record UpdateShiftRequest(string Name, string Schedule);
