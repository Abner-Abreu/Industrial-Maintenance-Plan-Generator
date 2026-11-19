using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Contracts.Systems;

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
    /// Expected outcomes (not found, still has dependencies) are reported through <see cref="DeleteResult"/>;
    /// any other failure throws.
    /// </summary>
    Task<DeleteResult> DeleteLineAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Edits the name and description of a system through the systems API (SIS-3). Administrator only.
    /// The operative state and the line are not changed.
    /// Throws <see cref="HttpRequestException"/> with 400 when the request is invalid, or 404 when the system does not exist.
    /// </summary>
    /// <returns>The updated system.</returns>
    Task<EquipmentSystemDto> UpdateSystemAsync(Guid systemId, UpdateSystemRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a system through the systems API (SIS-4). Administrator only.
    /// Expected outcomes (not found, still has tasks or sensors) are reported through <see cref="DeleteResult"/>;
    /// any other failure throws.
    /// </summary>
    Task<DeleteResult> DeleteSystemAsync(Guid systemId, CancellationToken ct = default);
}

/// <summary>Outcome of a delete operation on a line (LIN-5) or a system (SIS-4).</summary>
public enum DeleteResult
{
    /// <summary>The item was deleted.</summary>
    Deleted,

    /// <summary>The item does not exist (it may have been deleted already).</summary>
    NotFound,

    /// <summary>The item was not deleted because other objects still depend on it (HTTP 409).</summary>
    HasDependencies
}