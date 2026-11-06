using IndustrialPlanGenerator.Services.Contracts.Sensors;

namespace IndustrialPlanGenerator.Services.Sensors;

/// <summary>Temporary placeholder: the sensor endpoints respond 501 until <c>SensorService</c> exists. It is removed then, along with its registration in <c>DependencyInjection</c>.</summary>
internal sealed class SensorServiceStub : ISensorService
{
    private const string Pending = "El servicio de sensores aún no está implementado.";

    public Task<IReadOnlyList<SensorDto>> ListBySystemAsync(Guid systemId, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<SensorDto> CreateAsync(Guid systemId, CreateSensorRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
}
