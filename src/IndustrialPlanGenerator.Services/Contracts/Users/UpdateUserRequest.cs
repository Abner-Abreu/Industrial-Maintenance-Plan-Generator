namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>
/// Editable user data (USR-4). The role and password are not changed here.
/// </summary>
/// <param name="Name">New user name (unique, maximum 200 characters).</param>
/// <param name="Specialty">Required only for technicians; must be null for other roles.</param>
/// <param name="Age">Required only for operators (minimum 18); must be null for other roles.</param>
/// <param name="ShiftId">Required only for operators; must be null for other roles.</param>
public sealed record UpdateUserRequest(
    string Name,
    string? Specialty = null,
    int? Age = null,
    Guid? ShiftId = null);
