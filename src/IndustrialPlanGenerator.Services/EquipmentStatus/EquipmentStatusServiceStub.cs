using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;

namespace IndustrialPlanGenerator.Services.EquipmentStatus;

/// <summary>Marcador temporal: los endpoints de estado responden 501 hasta que exista <c>EquipmentStatusService</c>. Se borra entonces, junto con su registro en <c>DependencyInjection</c>.</summary>
internal sealed class EquipmentStatusServiceStub : IEquipmentStatusService
{
    private const string Pending = "El servicio de estado de equipos aún no está implementado.";

    public Task<EquipmentStatusDto> GetAsync(Guid systemId, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<IReadOnlyList<EquipmentStatusDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<EquipmentStatusDto> UpdateAsync(Guid systemId, UpdateEquipmentStatusRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
}
