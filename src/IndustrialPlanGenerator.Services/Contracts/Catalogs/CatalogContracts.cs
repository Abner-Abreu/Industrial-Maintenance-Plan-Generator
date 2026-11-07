namespace IndustrialPlanGenerator.Services.Contracts.Catalogs;

/// <summary>Work shift (CAT-1 to CAT-3).</summary>
/// <param name="Id">Shift ID.</param>
/// <param name="Name">Name (unique).</param>
/// <param name="Schedule">Schedule (unique), for example "06:00-14:00".</param>
public sealed record ShiftDto(Guid Id, string Name, string Schedule);

/// <summary>Data for creating a shift (CAT-2).</summary>
/// <param name="Name">Name (required and unique).</param>
/// <param name="Schedule">Schedule (required and unique).</param>
public sealed record CreateShiftRequest(string Name, string Schedule);

/// <summary>Data for editing a shift (CAT-3). Replaces both fields.</summary>
/// <param name="Name">New name (required and unique).</param>
/// <param name="Schedule">New schedule (required and unique).</param>
public sealed record UpdateShiftRequest(string Name, string Schedule);
