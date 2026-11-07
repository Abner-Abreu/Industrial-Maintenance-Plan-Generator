namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Technician authorized on a line (USR-8).</summary>
/// <param name="Id">Technician ID (the same as the user ID).</param>
/// <param name="Name">User name.</param>
/// <param name="Specialty">Specialty.</param>
/// <param name="Permission">Permission held on that line: <c>Generate</c> or <c>Validate</c>.</param>
public sealed record LineTechnicianDto(Guid Id, string Name, string Specialty, string Permission);
