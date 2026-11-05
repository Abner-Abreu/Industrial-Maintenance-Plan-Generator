namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Línea asignada a un usuario.</summary>
/// <param name="LineId">Id de la línea de producción.</param>
/// <param name="Permission">Solo técnicos: <c>Generate</c> o <c>Validate</c>. Nulo para operarios.</param>
public sealed record UserLineDto(Guid LineId, string? Permission);

/// <summary>Detalle de un usuario con los datos propios de su rol.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Name">Nombre de usuario.</param>
/// <param name="Role">Rol: Administrator, Technician, Operator o PlantManager.</param>
/// <param name="Specialty">Solo técnicos.</param>
/// <param name="Age">Solo operarios.</param>
/// <param name="ShiftId">Solo operarios.</param>
/// <param name="ShiftName">Solo operarios.</param>
/// <param name="Lines">Líneas asignadas (técnicos y operarios); vacío para el resto de roles.</param>
public sealed record UserDetailDto(
    Guid Id,
    string Name,
    string Role,
    string? Specialty,
    int? Age,
    Guid? ShiftId,
    string? ShiftName,
    IReadOnlyList<UserLineDto> Lines);
