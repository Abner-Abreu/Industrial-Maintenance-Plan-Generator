using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

/// <summary>
/// Data access used by the lines management page.
/// Implemented by the real HTTP service and by a fake used while the backend is not ready.
/// </summary>
public interface ILinesManagementService
{
    /// <summary>Loads the summaries of all visible lines (LIN-1). Systems are not included.</summary>
    Task<IReadOnlyList<LineSummaryDto>> LoadLinesAsync(CancellationToken ct = default);

    /// <summary>Loads the detail of a single line, including its systems (LIN-2).</summary>
    Task<LineDetailDto> LoadLineAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a production line (LIN-3). Administrator only.
    /// Throws <see cref="HttpRequestException"/> with 400 when the request is invalid.
    /// </summary>
    /// <returns>The detail of the newly created line.</returns>
    Task<LineDetailDto> CreateLineAsync(CreateLineRequest request, CancellationToken ct = default);

    /// <summary>
    /// Edits the name and productive process of a line (LIN-4). Administrator only.
    /// Throws <see cref="HttpRequestException"/> with 400 when the request is invalid, or 404 when the line does not exist.
    /// </summary>
    /// <returns>The detail of the updated line.</returns>
    Task<LineDetailDto> UpdateLineAsync(Guid id, UpdateLineRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a line (LIN-5). Administrator only.
    /// Throws <see cref="HttpRequestException"/> with 404 when the line does not exist,
    /// or 409 when it still has systems, plans, or assignments.
    /// </summary>
    Task DeleteLineAsync(Guid id, CancellationToken ct = default);
}