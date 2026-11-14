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
}