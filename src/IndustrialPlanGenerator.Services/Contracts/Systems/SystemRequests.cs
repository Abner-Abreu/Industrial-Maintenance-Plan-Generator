namespace IndustrialPlanGenerator.Services.Contracts.Systems;

/// <summary>Data for creating a system or equipment (SIS-2). The operative state is assigned by the service, not the client.</summary>
/// <param name="LineId">Line it belongs to (must exist).</param>
/// <param name="Name">Name (required).</param>
/// <param name="Description">Description (required).</param>
public sealed record CreateSystemRequest(Guid LineId, string Name, string Description);

/// <summary>Data for editing a system (SIS-3). Does not change the line or the operative state.</summary>
/// <param name="Name">New name (required).</param>
/// <param name="Description">New description (required).</param>
public sealed record UpdateSystemRequest(string Name, string Description);
