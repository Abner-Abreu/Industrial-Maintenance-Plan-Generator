namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>User as shown in listings.</summary>
/// <param name="Id">Identifier.</param>
/// <param name="Name">User name.</param>
/// <param name="Role">Role: Administrator, Technician, Operator, or PlantManager.</param>
public sealed record UserSummaryDto(Guid Id, string Name, string Role);
