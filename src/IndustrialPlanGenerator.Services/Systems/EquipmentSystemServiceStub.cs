using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Contracts.Systems;

namespace IndustrialPlanGenerator.Services.Systems;

/// <summary>
/// Marcador temporal: los endpoints de sistemas responden 501 hasta que exista la implementación real.
/// Se borra, junto con su línea en <c>DependencyInjection</c>, cuando se añada el servicio real.
/// </summary>
internal sealed class EquipmentSystemServiceStub : IEquipmentSystemService
{
    private const string Pending = "El servicio de sistemas aún no está implementado.";

    public Task<IReadOnlyList<EquipmentSystemDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<EquipmentSystemDto> CreateAsync(CreateSystemRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<EquipmentSystemDto> UpdateAsync(Guid id, UpdateSystemRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException(Pending);
}
