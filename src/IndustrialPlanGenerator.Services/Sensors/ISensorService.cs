using IndustrialPlanGenerator.Services.Contracts.Sensors;

namespace IndustrialPlanGenerator.Services.Sensors;

/// <summary>Sensor service (SEN-1, SEN-2). There is no deletion: historical readings depend on the sensor.</summary>
public interface ISensorService
{
    /// <summary>Sensors of a system (SEN-1).</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The system does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ForbiddenException">Technician without access to the system's line (HTTP 403).</exception>
    Task<IReadOnlyList<SensorDto>> ListBySystemAsync(Guid systemId, CancellationToken ct = default);

    /// <summary>Registers a sensor in a system (SEN-2).</summary>
    /// <exception cref="Exceptions.InvalidRequestException">Empty or invalid type (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The system does not exist (HTTP 404).</exception>
    Task<SensorDto> CreateAsync(Guid systemId, CreateSensorRequest request, CancellationToken ct = default);
}
