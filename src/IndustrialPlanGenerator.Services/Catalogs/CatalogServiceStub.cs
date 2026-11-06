using IndustrialPlanGenerator.Services.Contracts.Catalogs;

namespace IndustrialPlanGenerator.Services.Catalogs;

/// <summary>
/// Marcador temporal: los endpoints de turnos responden 501 hasta que exista <c>CatalogService</c>.
/// Los tipos de mantenimiento y las criticidades ya funcionan porque son valores fijos del dominio.
/// Se borra, junto con su registro en <c>DependencyInjection</c>, cuando se añada el servicio real.
/// </summary>
internal sealed class CatalogServiceStub : ICatalogService
{
    private const string Pending = "El servicio de catálogos aún no está implementado.";

    public Task<IReadOnlyList<ShiftDto>> ListShiftsAsync(CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task DeleteShiftAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException(Pending);

    public IReadOnlyList<string> ListMaintenanceTypes()
        => Enum.GetNames<Domain.Enums.MaintenanceType>();

    public IReadOnlyList<string> ListCriticalities()
        => Enum.GetNames<Domain.Enums.Criticality>();
}
