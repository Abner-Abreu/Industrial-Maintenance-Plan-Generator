using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;

namespace IndustrialPlanGenerator.Services.EquipmentStatus;

/// <summary>
/// Servicio de estado operativo de equipos (EST-1 a EST-3). Lee de la caché y, si falta, de PostgreSQL;
/// al actualizar escribe primero en PostgreSQL (fuente de verdad) y después refresca la caché.
/// </summary>
public interface IEquipmentStatusService
{
    /// <summary>Estado de un equipo (EST-1).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">El sistema no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">Sin acceso a la línea del equipo (HTTP 403).</exception>
    Task<EquipmentStatusDto> GetAsync(Guid systemId, CancellationToken ct = default);

    /// <summary>Estado de todos los equipos de una línea (EST-2), ordenados por nombre.</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">La línea no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">Sin acceso a la línea (HTTP 403).</exception>
    Task<IReadOnlyList<EquipmentStatusDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default);

    /// <summary>Cambia el estado de un equipo (EST-3).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Estado no válido (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">El sistema no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">Técnico sin acceso a la línea del equipo (HTTP 403).</exception>
    Task<EquipmentStatusDto> UpdateAsync(Guid systemId, UpdateEquipmentStatusRequest request, CancellationToken ct = default);
}
