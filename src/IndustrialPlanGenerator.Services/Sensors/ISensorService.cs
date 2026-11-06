using IndustrialPlanGenerator.Services.Contracts.Sensors;

namespace IndustrialPlanGenerator.Services.Sensors;

/// <summary>Servicio de sensores (SEN-1, SEN-2). No existe baja: las lecturas históricas dependen del sensor.</summary>
public interface ISensorService
{
    /// <summary>Sensores de un sistema (SEN-1).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">El sistema no existe (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">Técnico sin acceso a la línea del sistema (HTTP 403).</exception>
    Task<IReadOnlyList<SensorDto>> ListBySystemAsync(Guid systemId, CancellationToken ct = default);

    /// <summary>Da de alta un sensor en un sistema (SEN-2).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Tipo vacío o no válido (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">El sistema no existe (HTTP 404).</exception>
    Task<SensorDto> CreateAsync(Guid systemId, CreateSensorRequest request, CancellationToken ct = default);
}
