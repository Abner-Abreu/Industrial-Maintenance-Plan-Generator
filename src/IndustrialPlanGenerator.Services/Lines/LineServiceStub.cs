using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Services.Lines;

/// <summary>
/// Temporary placeholder so the line endpoints respond 501 until the real implementation exists.
/// It is removed, along with its line in <c>DependencyInjection</c>, when <c>LineService</c> is added.
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
