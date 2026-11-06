namespace IndustrialPlanGenerator.Services.Contracts.Systems;

/// <summary>Datos para crear un sistema o equipo (SIS-2). El estado operativo lo asigna el servicio, no el cliente.</summary>
/// <param name="LineId">Línea a la que pertenece (debe existir).</param>
/// <param name="Name">Nombre (obligatorio).</param>
/// <param name="Description">Descripción (obligatoria).</param>
public sealed record CreateSystemRequest(Guid LineId, string Name, string Description);

/// <summary>Datos para editar un sistema (SIS-3). No cambia la línea ni el estado operativo.</summary>
/// <param name="Name">Nuevo nombre (obligatorio).</param>
/// <param name="Description">Nueva descripción (obligatoria).</param>
public sealed record UpdateSystemRequest(string Name, string Description);
