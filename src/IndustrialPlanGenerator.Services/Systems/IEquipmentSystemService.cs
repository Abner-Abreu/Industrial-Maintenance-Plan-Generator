using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Contracts.Systems;

namespace IndustrialPlanGenerator.Services.Systems;

/// <summary>System or equipment service of a line (SIS-1 to SIS-4). It is part of service S4, together with lines.</summary>
public interface IEquipmentSystemService
{
    /// <summary>Systems of a line (SIS-1), ordered by name.</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The line does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">The user does not have access to the line (HTTP 403).</exception>
    Task<IReadOnlyList<EquipmentSystemDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default);

    /// <summary>Creates a system in a line (SIS-2).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Empty name or description (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The line does not exist (HTTP 404).</exception>
    Task<EquipmentSystemDto> CreateAsync(CreateSystemRequest request, CancellationToken ct = default);

    /// <summary>Edits the name and description (SIS-3). The operative state is not touched here.</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Empty name or description (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The system does not exist (HTTP 404).</exception>
    Task<EquipmentSystemDto> UpdateAsync(Guid id, UpdateSystemRequest request, CancellationToken ct = default);

    /// <summary>Deletes a system (SIS-4).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The system does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">It has tasks or sensors (HTTP 409).</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
