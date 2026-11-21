using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

namespace IndustrialPlanGenerator.Web.Client.Services.CatalogsService;

/// <summary>
/// Data access used by the catalogs page.
/// Implemented by the real HTTP service and by a fake used while the backend is not ready.
/// </summary>
public interface ICatalogsService
{
    /// <summary>Loads all the shifts (CAT-1).</summary>
    Task<IReadOnlyList<ShiftDto>> LoadShiftsAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a shift (CAT-2). Administrator only.
    /// Throws <see cref="HttpRequestException"/> with 400 when the request is invalid,
    /// or 409 when the name or the schedule already exists.
    /// </summary>
    /// <returns>The newly created shift.</returns>
    Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request, CancellationToken ct = default);

    /// <summary>
    /// Edits the name and schedule of a shift (CAT-3). Administrator only.
    /// Throws <see cref="HttpRequestException"/> with 400 when the request is invalid,
    /// 404 when the shift does not exist, or 409 when the name or the schedule already exists.
    /// </summary>
    /// <returns>The updated shift.</returns>
    Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a shift (CAT-4). Administrator only.
    /// Expected outcomes (not found, still has operators) are reported through <see cref="DeleteResult"/>;
    /// any other failure throws.
    /// </summary>
    Task<DeleteResult> DeleteShiftAsync(Guid id, CancellationToken ct = default);

    /// <summary>Loads the maintenance types: fixed domain values (CAT-5).</summary>
    Task<IReadOnlyList<string>> LoadMaintenanceTypesAsync(CancellationToken ct = default);

    /// <summary>Loads the criticality levels: fixed domain values (CAT-6).</summary>
    Task<IReadOnlyList<string>> LoadCriticalitiesAsync(CancellationToken ct = default);
}