using IndustrialPlanGenerator.Services.Contracts.Catalogs;

namespace IndustrialPlanGenerator.Services.Catalogs;

/// <summary>Servicio de catálogos (CAT-1 a CAT-6): turnos (CRUD) y valores fijos de tipos de mantenimiento y criticidades.</summary>
public interface ICatalogService
{
    /// <summary>Turnos, ordenados por nombre (CAT-1).</summary>
    Task<IReadOnlyList<ShiftDto>> ListShiftsAsync(CancellationToken ct = default);

    /// <summary>Crea un turno (CAT-2).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Nombre u horario vacío (HTTP 400).</exception>
    /// <exception cref="Exceptions.ConflictException">Ya existe un turno con ese nombre u horario (HTTP 409).</exception>
    Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request, CancellationToken ct = default);

    /// <summary>Edita un turno (CAT-3).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Nombre u horario vacío (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">El turno no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">Otro turno ya tiene ese nombre u horario (HTTP 409).</exception>
    Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftRequest request, CancellationToken ct = default);

    /// <summary>Elimina un turno (CAT-4).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">El turno no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">Tiene operarios asignados (HTTP 409).</exception>
    Task DeleteShiftAsync(Guid id, CancellationToken ct = default);

    /// <summary>Valores del enum <c>MaintenanceType</c> (CAT-5).</summary>
    IReadOnlyList<string> ListMaintenanceTypes();

    /// <summary>Valores del enum <c>Criticality</c> (CAT-6).</summary>
    IReadOnlyList<string> ListCriticalities();
}
