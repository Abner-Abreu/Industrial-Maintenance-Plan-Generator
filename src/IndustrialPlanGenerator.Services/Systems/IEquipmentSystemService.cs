using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Contracts.Systems;

namespace IndustrialPlanGenerator.Services.Systems;

/// <summary>Servicio de sistemas o equipos de una línea (SIS-1 a SIS-4). Forma parte del servicio S4, junto con las líneas.</summary>
public interface IEquipmentSystemService
{
    /// <summary>Sistemas de una línea (SIS-1), ordenados por nombre.</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">La línea no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">El usuario no tiene acceso a la línea (HTTP 403).</exception>
    Task<IReadOnlyList<EquipmentSystemDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default);

    /// <summary>Crea un sistema en una línea (SIS-2).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Nombre o descripción vacíos (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">La línea no existe (HTTP 404).</exception>
    Task<EquipmentSystemDto> CreateAsync(CreateSystemRequest request, CancellationToken ct = default);

    /// <summary>Edita nombre y descripción (SIS-3). El estado operativo no se toca aquí.</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Nombre o descripción vacíos (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">El sistema no existe (HTTP 404).</exception>
    Task<EquipmentSystemDto> UpdateAsync(Guid id, UpdateSystemRequest request, CancellationToken ct = default);

    /// <summary>Elimina un sistema (SIS-4).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">El sistema no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">Tiene tareas o sensores (HTTP 409).</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
