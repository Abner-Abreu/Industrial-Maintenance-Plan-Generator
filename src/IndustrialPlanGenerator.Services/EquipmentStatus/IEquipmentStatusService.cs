using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;

namespace IndustrialPlanGenerator.Services.EquipmentStatus;

/// <summary>
/// Equipment operational status service (EST-1 to EST-3). Reads from the cache and, if missing, from PostgreSQL;
/// when updating, it writes to PostgreSQL first (source of truth) and then refreshes the cache.
/// </summary>
public interface IEquipmentStatusService
{
    /// <summary>State of an equipment (EST-1).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The system does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">No access to the equipment's line (HTTP 403).</exception>
    Task<EquipmentStatusDto> GetAsync(Guid systemId, CancellationToken ct = default);

    /// <summary>State of all the equipment of a line (EST-2), ordered by name.</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The line does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">No access to the line (HTTP 403).</exception>
    Task<IReadOnlyList<EquipmentStatusDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default);

    /// <summary>Changes the state of an equipment (EST-3).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Invalid state (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The system does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">Technician without access to the equipment's line (HTTP 403).</exception>
    Task<EquipmentStatusDto> UpdateAsync(Guid systemId, UpdateEquipmentStatusRequest request, CancellationToken ct = default);
}
