namespace IndustrialPlanGenerator.Services.Contracts.Auth;

/// <summary>Datos del usuario autenticado que el front usa para armar el menú.</summary>
/// <param name="Id">Identificador del usuario.</param>
/// <param name="Name">Nombre del usuario.</param>
/// <param name="Role">
/// Rol como texto: <c>Administrator</c>, <c>Technician</c>, <c>Operator</c> o <c>PlantManager</c>.
/// Es texto (y no el enum del Dominio) para que el JSON y el token no dependan de ese enum.
/// </param>
public sealed record SessionUserDto(Guid Id, string Name, string Role);
