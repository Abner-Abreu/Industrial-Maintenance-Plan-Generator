namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Data for creating a user and its role record (USR-3).</summary>
/// <param name="Name">User name (unique, maximum 200 characters).</param>
/// <param name="Password">Initial password (minimum 8 characters).</param>
/// <param name="Role">Administrator, Technician, Operator, or PlantManager.</param>
/// <param name="Specialty">Required only for technicians; must be null for other roles.</param>
/// <param name="Age">Required only for operators (minimum 18); must be null for other roles.</param>
/// <param name="ShiftId">Required only for operators; must be null for other roles.</param>
public sealed record CreateUserRequest(
    string Name,
    string Password,
    string Role,
    string? Specialty = null,
    int? Age = null,
    Guid? ShiftId = null);
