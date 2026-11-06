using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Contracts.Systems;

namespace IndustrialPlanGenerator.Services.Systems;

/// <summary>
/// Temporary placeholder: the system endpoints respond 501 until the real implementation exists.
/// It is removed, along with its line in <c>DependencyInjection</c>, when the real service is added.
/// </summary>
internal sealed class EquipmentSystemServiceStub : IEquipmentSystemService
{
    private const string Pending = "El servicio de sistemas aún no está implementado.";

    public Task<IReadOnlyList<EquipmentSystemDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<EquipmentSystemDto> CreateAsync(CreateSystemRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<EquipmentSystemDto> UpdateAsync(Guid id, UpdateSystemRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException(Pending);
}
