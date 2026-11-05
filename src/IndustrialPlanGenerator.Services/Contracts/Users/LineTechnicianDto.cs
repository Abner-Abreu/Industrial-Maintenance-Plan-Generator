namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Técnico autorizado en una línea (USR-8).</summary>
/// <param name="Id">Id del técnico (el mismo que el del usuario).</param>
/// <param name="Name">Nombre de usuario.</param>
/// <param name="Specialty">Especialidad.</param>
/// <param name="Permission">Permiso que tiene en esa línea: <c>Generate</c> o <c>Validate</c>.</param>
public sealed record LineTechnicianDto(Guid Id, string Name, string Specialty, string Permission);
