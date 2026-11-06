using IndustrialPlanGenerator.Services.Contracts.Catalogs;

namespace IndustrialPlanGenerator.Services.Catalogs;

/// <summary>Catalog service (CAT-1 to CAT-6): shifts (CRUD) and fixed values for maintenance types and criticalities.</summary>
public interface ICatalogService
{
    /// <summary>Shifts, ordered by name (CAT-1).</summary>
    Task<IReadOnlyList<ShiftDto>> ListShiftsAsync(CancellationToken ct = default);

    /// <summary>Creates a shift (CAT-2).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Empty name or schedule (HTTP 400).</exception>
    /// <exception cref="Exceptions.ConflictException">A shift with that name or schedule already exists (HTTP 409).</exception>
    Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request, CancellationToken ct = default);

    /// <summary>Edits a shift (CAT-3).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Empty name or schedule (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The shift does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">Another shift already has that name or schedule (HTTP 409).</exception>
    Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftRequest request, CancellationToken ct = default);

    /// <summary>Deletes a shift (CAT-4).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The shift does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">It has assigned operators (HTTP 409).</exception>
    Task DeleteShiftAsync(Guid id, CancellationToken ct = default);

    /// <summary>Values of the <c>MaintenanceType</c> enum (CAT-5).</summary>
    IReadOnlyList<string> ListMaintenanceTypes();

    /// <summary>Values of the <c>Criticality</c> enum (CAT-6).</summary>
    IReadOnlyList<string> ListCriticalities();
}
