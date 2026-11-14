using System.Net;
using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

/// <summary>
/// In-memory placeholder for ILinesManagementService, used while the backend services are not implemented.
/// It does not touch the network or the JWT. Remove its registration once the real API is available.
/// </summary>
public sealed class FakeLinesManagementService : ILinesManagementService
{
    // Simulated network latency so loading states can be seen in the UI.
    private static readonly TimeSpan Latency = TimeSpan.FromMilliseconds(400);

    // Source of truth: full details. Summaries are derived from it, like the real API does.
    private readonly List<LineDetailDto> _lines =
    [
        new(Id(1), "Línea de envasado A", "Envasado", new List<EquipmentSystemDto>
        {
            new(Id(101), "Llenadora", "Dosifica el producto en los envases.", "Operational"),
            new(Id(102), "Tapadora", "Coloca y aprieta las tapas.", "Stopped"),
            new(Id(103), "Etiquetadora", "Aplica la etiqueta frontal y trasera.", "Operational"),
            new(Id(104), "Cinta transportadora", "Transporte entre estaciones.", "Maintenance")
        }),
        new(Id(2), "Línea de mezclado", "Mezclado", new List<EquipmentSystemDto>
        {
            new(Id(201), "Mezcladora principal", "Mezcla los ingredientes base.", "Operational"),
            new(Id(202), "Bomba de trasvase", "Traslada la mezcla al tanque de reposo.", "Operational")
        }),
        // A line without systems, to exercise the empty state.
        new(Id(3), "Línea de pruebas", "Control de calidad", new List<EquipmentSystemDto>())
    ];

    /// <inheritdoc />
    public async Task<IReadOnlyList<LineSummaryDto>> LoadLinesAsync(CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        return [.. _lines.Select(line =>
            new LineSummaryDto(line.Id, line.Name, line.ProductiveProcess, line.Systems.Count))];
    }

    /// <inheritdoc />
    public async Task<LineDetailDto> LoadLineAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        // Mimics the real API contract: 404 when the line does not exist.
        return _lines.FirstOrDefault(line => line.Id == id)
               ?? throw new HttpRequestException($"Line {id} not found.", null, HttpStatusCode.NotFound);
    }

    // Deterministic IDs so the sample data is stable between runs.
    private static Guid Id(int n) => new($"00000000-0000-0000-0000-{n:D12}");
}