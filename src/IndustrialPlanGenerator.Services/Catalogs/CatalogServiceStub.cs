using IndustrialPlanGenerator.Services.Contracts.Catalogs;

namespace IndustrialPlanGenerator.Services.Catalogs;

/// <summary>
/// Temporary placeholder: the shift endpoints respond 501 until <c>CatalogService</c> exists.
/// Maintenance types and criticalities already work because they are fixed domain values.
/// It is removed, along with its registration in <c>DependencyInjection</c>, when the real service is added.
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
