namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Datos para crear un usuario y su registro de rol (USR-3).</summary>
/// <param name="Name">Nombre de usuario (único, máximo 200 caracteres).</param>
/// <param name="Password">Contraseña inicial (mínimo 8 caracteres).</param>
/// <param name="Role">Administrator, Technician, Operator o PlantManager.</param>
/// <param name="Specialty">Obligatoria solo para técnicos; debe ser nula en el resto.</param>
/// <param name="Age">Obligatoria solo para operarios (mínimo 18); debe ser nula en el resto.</param>
/// <param name="ShiftId">Obligatorio solo para operarios; debe ser nulo en el resto.</param>
public sealed record CreateUserRequest(
    string Name,
    string Password,
    string Role,
    string? Specialty = null,
    int? Age = null,
    Guid? ShiftId = null);
