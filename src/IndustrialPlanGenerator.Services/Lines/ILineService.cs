using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Services.Lines;

/// <summary>Servicio de líneas de producción (LIN-1 a LIN-5). El usuario de la sesión se obtiene de <c>ICurrentUser</c>.</summary>
public interface ILineService
{
    /// <summary>
    /// Líneas visibles para el usuario actual: administrador y jefe de planta, todas; técnico y operario,
    /// solo las suyas (LIN-1). Ordenadas por nombre.
    /// </summary>
    Task<IReadOnlyList<LineSummaryDto>> ListAsync(CancellationToken ct = default);

    /// <summary>Detalle de una línea con sus sistemas (LIN-2).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">La línea no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">El usuario no tiene acceso a la línea (HTTP 403).</exception>
    Task<LineDetailDto> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>Crea una línea (LIN-3).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Nombre o proceso vacío (HTTP 400).</exception>
    Task<LineDetailDto> CreateAsync(CreateLineRequest request, CancellationToken ct = default);

    /// <summary>Edita nombre y proceso productivo de una línea (LIN-4).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Nombre o proceso vacío (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">La línea no existe (HTTP 404).</exception>
    Task<LineDetailDto> UpdateAsync(Guid id, UpdateLineRequest request, CancellationToken ct = default);

    /// <summary>Elimina una línea (LIN-5).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">La línea no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">Tiene sistemas, planes o asignaciones (HTTP 409).</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
