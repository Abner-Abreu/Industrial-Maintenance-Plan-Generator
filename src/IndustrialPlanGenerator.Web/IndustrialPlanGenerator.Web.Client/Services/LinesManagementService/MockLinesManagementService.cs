using System.Net;
using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

/// <summary>
/// In-memory placeholder for ILinesManagementService, used while the backend services are not implemented.
/// It does not touch the network or the JWT. Remove its registration once the real API is available.
/// Changes are kept in memory only and are lost on page reload.
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
        // A line without systems, to exercise the empty state and to be deletable.
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
               ?? throw NotFound(id);
    }

    /// <inheritdoc />
    public async Task<LineDetailDto> CreateLineAsync(CreateLineRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await Task.Delay(Latency, ct);

        // Mimics the real API contract: 400 when the request is invalid.
        EnsureValid(request.Name, request.ProductiveProcess);

        var created = new LineDetailDto(
            Guid.NewGuid(),
            request.Name.Trim(),
            request.ProductiveProcess.Trim(),
            new List<EquipmentSystemDto>());

        _lines.Add(created);
        return created;
    }

    /// <inheritdoc />
    public async Task<LineDetailDto> UpdateLineAsync(Guid id, UpdateLineRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await Task.Delay(Latency, ct);

        // Mimics the real API contract: 404 when the line does not exist, 400 when the request is invalid.
        var index = _lines.FindIndex(line => line.Id == id);
        if (index < 0)
        {
            throw NotFound(id);
        }

        EnsureValid(request.Name, request.ProductiveProcess);

        // Systems are untouched by this operation.
        var updated = _lines[index] with
        {
            Name = request.Name.Trim(),
            ProductiveProcess = request.ProductiveProcess.Trim()
        };

        _lines[index] = updated;
        return updated;
    }

    /// <inheritdoc />
    public async Task<DeleteLineResult> DeleteLineAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        // Mimics the real service: the API's 404 and 409 are reported as results.
        var line = _lines.FirstOrDefault(l => l.Id == id);
        if (line is null)
        {
            return DeleteLineResult.NotFound;
        }

        // The fake has no plans or assignments, only systems.
        if (line.Systems.Count > 0)
        {
            return DeleteLineResult.HasDependencies;
        }

        _lines.Remove(line);
        return DeleteLineResult.Deleted;
    }

    private static HttpRequestException NotFound(Guid id)
        => new($"Line {id} not found.", null, HttpStatusCode.NotFound);

    private static void EnsureValid(string? name, string? productiveProcess)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(productiveProcess))
        {
            throw new HttpRequestException(
                "Name and productive process are required.", null, HttpStatusCode.BadRequest);
        }
    }

    // Deterministic IDs so the sample data is stable between runs.
    private static Guid Id(int n) => new($"00000000-0000-0000-0000-{n:D12}");
}