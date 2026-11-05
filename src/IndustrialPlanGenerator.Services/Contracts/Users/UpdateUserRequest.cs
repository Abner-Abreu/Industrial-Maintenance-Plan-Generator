namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>
/// Datos editables de un usuario (USR-4). El rol y la contraseña no se cambian aquí.
/// </summary>
/// <param name="Name">Nuevo nombre de usuario (único, máximo 200 caracteres).</param>
/// <param name="Specialty">Obligatoria solo para técnicos; debe ser nula en el resto.</param>
/// <param name="Age">Obligatoria solo para operarios (mínimo 18); debe ser nula en el resto.</param>
/// <param name="ShiftId">Obligatorio solo para operarios; debe ser nulo en el resto.</param>
public sealed record UpdateUserRequest(
    string Name,
    string? Specialty = null,
    int? Age = null,
    Guid? ShiftId = null);
