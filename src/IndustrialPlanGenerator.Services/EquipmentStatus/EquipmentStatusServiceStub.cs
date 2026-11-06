using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;

namespace IndustrialPlanGenerator.Services.EquipmentStatus;

/// <summary>Temporary placeholder: the status endpoints respond 501 until <c>EquipmentStatusService</c> exists. It is then removed, along with its registration in <c>DependencyInjection</c>.</summary>
internal sealed class EquipmentStatusServiceStub : IEquipmentStatusService
{
    private const string Pending = "El servicio de estado de equipos aún no está implementado.";

    public Task<EquipmentStatusDto> GetAsync(Guid systemId, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<IReadOnlyList<EquipmentStatusDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<EquipmentStatusDto> UpdateAsync(Guid systemId, UpdateEquipmentStatusRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
}
