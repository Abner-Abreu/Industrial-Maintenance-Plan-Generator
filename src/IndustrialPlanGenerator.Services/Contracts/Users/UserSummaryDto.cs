namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Usuario tal como aparece en los listados.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Name">Nombre de usuario.</param>
/// <param name="Role">Rol: Administrator, Technician, Operator o PlantManager.</param>
public sealed record UserSummaryDto(Guid Id, string Name, string Role);
