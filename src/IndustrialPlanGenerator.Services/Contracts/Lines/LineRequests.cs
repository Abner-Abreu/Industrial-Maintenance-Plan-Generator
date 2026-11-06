namespace IndustrialPlanGenerator.Services.Contracts.Lines;

/// <summary>Data for creating a line (LIN-3).</summary>
/// <param name="Name">Line name (required).</param>
/// <param name="ProductiveProcess">Productive process (required).</param>
public sealed record CreateLineRequest(string Name, string ProductiveProcess);

/// <summary>Data for editing a line (LIN-4). Replaces both fields.</summary>
/// <param name="Name">New name (required).</param>
/// <param name="ProductiveProcess">New productive process (required).</param>
public sealed record UpdateLineRequest(string Name, string ProductiveProcess);
