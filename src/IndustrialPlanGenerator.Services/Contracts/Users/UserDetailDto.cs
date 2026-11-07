namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Line assigned to a user.</summary>
/// <param name="LineId">Production line ID.</param>
/// <param name="Permission">Technicians only: <c>Generate</c> or <c>Validate</c>. Null for operators.</param>
public sealed record UserLineDto(Guid LineId, string? Permission);

/// <summary>User details including the data specific to their role.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="Name">User name.</param>
/// <param name="Role">Role: Administrator, Technician, Operator, or PlantManager.</param>
/// <param name="Specialty">Technicians only.</param>
/// <param name="Age">Operators only.</param>
/// <param name="ShiftId">Operators only.</param>
/// <param name="ShiftName">Operators only.</param>
/// <param name="Lines">Assigned lines (technicians and operators); empty for other roles.</param>
public sealed record UserDetailDto(
    Guid Id,
    string Name,
    string Role,
    string? Specialty,
    int? Age,
    Guid? ShiftId,
    string? ShiftName,
    IReadOnlyList<UserLineDto> Lines);
