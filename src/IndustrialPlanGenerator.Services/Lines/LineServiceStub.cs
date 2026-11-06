using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Services.Lines;

/// <summary>
/// Marcador temporal para que los endpoints de líneas respondan 501 hasta que exista la implementación real.
/// Se borra, junto con su línea en <c>DependencyInjection</c>, cuando se añada <c>LineService</c>.
/// </summary>
internal sealed class LineServiceStub : ILineService
{
    private const string Pending = "El servicio de líneas aún no está implementado.";

    public Task<IReadOnlyList<LineSummaryDto>> ListAsync(CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<LineDetailDto> GetAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<LineDetailDto> CreateAsync(CreateLineRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task<LineDetailDto> UpdateAsync(Guid id, UpdateLineRequest request, CancellationToken ct = default) => throw new NotImplementedException(Pending);
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException(Pending);
}
