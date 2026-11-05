namespace IndustrialPlanGenerator.Services.Contracts.Common;

/// <summary>Una página de resultados de un listado.</summary>
/// <param name="Items">Elementos de la página.</param>
/// <param name="Page">Número de página (empieza en 1).</param>
/// <param name="PageSize">Tamaño de página aplicado.</param>
/// <param name="TotalCount">Total de elementos que cumplen el filtro, en todas las páginas.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
