namespace IndustrialPlanGenerator.Services.Contracts.Lines;

/// <summary>Datos para crear una línea (LIN-3).</summary>
/// <param name="Name">Nombre de la línea (obligatorio).</param>
/// <param name="ProductiveProcess">Proceso productivo (obligatorio).</param>
public sealed record CreateLineRequest(string Name, string ProductiveProcess);

/// <summary>Datos para editar una línea (LIN-4). Reemplaza ambos campos.</summary>
/// <param name="Name">Nuevo nombre (obligatorio).</param>
/// <param name="ProductiveProcess">Nuevo proceso productivo (obligatorio).</param>
public sealed record UpdateLineRequest(string Name, string ProductiveProcess);
