using IndustrialPlanGenerator.Services.Contracts.Sensors;

namespace IndustrialPlanGenerator.Services.Sensors;

/// <summary>Marcador temporal: los endpoints de sensores responden 501 hasta que exista <c>SensorService</c>. Se borra entonces, junto con su registro en <c>DependencyInjection</c>.</summary>
internal sealed class SensorServiceStub : ISensorService
{
    private const string Pending = "El servicio de sensores aún no está implementado.";

    public Task<IReadOnlyList<SensorDto>> ListBySystemAsync(Guid systemId, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<SensorDto> CreateAsync(Guid systemId, CreateSensorRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
}
