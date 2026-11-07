namespace IndustrialPlanGenerator.Services.Contracts.Common;

/// <summary>A page of results from a listing.</summary>
/// <param name="Items">Items on the page.</param>
/// <param name="Page">Page number (starts at 1).</param>
/// <param name="PageSize">Applied page size.</param>
/// <param name="TotalCount">Total number of items matching the filter across all pages.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
