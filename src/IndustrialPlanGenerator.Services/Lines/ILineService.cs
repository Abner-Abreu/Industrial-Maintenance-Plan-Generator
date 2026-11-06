using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Services.Lines;

/// <summary>Production line service (LIN-1 to LIN-5). The session user is obtained from <c>ICurrentUser</c>.</summary>
public interface ILineService
{
    /// <summary>
    /// Lines visible to the current user: administrator and plant manager, all lines; technician and operator,
    /// only their own (LIN-1). Ordered by name.
    /// </summary>
    Task<IReadOnlyList<LineSummaryDto>> ListAsync(CancellationToken ct = default);

    /// <summary>Line details including its systems (LIN-2).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The line does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">The user does not have access to the line (HTTP 403).</exception>
    Task<LineDetailDto> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>Creates a line (LIN-3).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Empty name or process (HTTP 400).</exception>
    Task<LineDetailDto> CreateAsync(CreateLineRequest request, CancellationToken ct = default);

    /// <summary>Edits the name and productive process of a line (LIN-4).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Empty name or process (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The line does not exist (HTTP 404).</exception>
    Task<LineDetailDto> UpdateAsync(Guid id, UpdateLineRequest request, CancellationToken ct = default);

    /// <summary>Deletes a line (LIN-5).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The line does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">It has systems, plans, or assignments (HTTP 409).</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
