using System.Net;
using System.Text.RegularExpressions;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

namespace IndustrialPlanGenerator.Web.Client.Services.CatalogsService;

/// <summary>
/// In-memory fake of <see cref="ICatalogsService"/> used while the backend is not ready.
/// Data lives as long as the instance, so register it as a singleton to keep changes between pages.
/// Reproduces the API rules: 400 for invalid data, 409 for duplicates, 404 for unknown ids,
/// and <see cref="DeleteResult.HasDependencies"/> for shifts that still have operators.
/// </summary>
public sealed class MockCatalogsService : ICatalogsService
{
    private static readonly TimeSpan Latency = TimeSpan.FromMilliseconds(250);

    private static readonly Regex ScheduleFormat =
        new(@"^([01]\d|2[0-3]):[0-5]\d-([01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);

    private static readonly Guid MorningId = Guid.Parse("a1b2c3d4-0001-4000-8000-000000000001");
    private static readonly Guid AfternoonId = Guid.Parse("a1b2c3d4-0002-4000-8000-000000000002");
    private static readonly Guid NightId = Guid.Parse("a1b2c3d4-0003-4000-8000-000000000003");

    private readonly List<ShiftDto> _shifts =
    [
        new(MorningId, "Morning", "06:00-14:00"),
        new(AfternoonId, "Afternoon", "14:00-22:00"),
        new(NightId, "Night", "22:00-06:00"),
    ];

    /// <summary>Shifts that "have operators", so deleting them reports <see cref="DeleteResult.HasDependencies"/>.</summary>
    private readonly HashSet<Guid> _shiftsWithOperators = [NightId];

    private static readonly string[] MaintenanceTypes = ["Preventive", "Corrective", "Predictive"];
    private static readonly string[] Criticalities = ["Low", "Medium", "High"];

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShiftDto>> LoadShiftsAsync(CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return _shifts.ToList();
    }

    /// <inheritdoc />
    public async Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        var (name, schedule) = Normalize(request.Name, request.Schedule);
        EnsureNoDuplicates(name, schedule, ignoreId: null);

        var created = new ShiftDto(Guid.NewGuid(), name, schedule);
        _shifts.Add(created);
        return created;
    }

    /// <inheritdoc />
    public async Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftRequest request, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        var index = _shifts.FindIndex(s => s.Id == id);
        if (index < 0)
        {
            throw Error(HttpStatusCode.NotFound, "The shift does not exist.");
        }

        var (name, schedule) = Normalize(request.Name, request.Schedule);
        EnsureNoDuplicates(name, schedule, ignoreId: id);

        var updated = new ShiftDto(id, name, schedule);
        _shifts[index] = updated;
        return updated;
    }

    /// <inheritdoc />
    public async Task<DeleteResult> DeleteShiftAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        var index = _shifts.FindIndex(s => s.Id == id);
        if (index < 0)
        {
            return DeleteResult.NotFound;
        }

        if (_shiftsWithOperators.Contains(id))
        {
            return DeleteResult.HasDependencies;
        }

        _shifts.RemoveAt(index);
        return DeleteResult.Deleted;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> LoadMaintenanceTypesAsync(CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return MaintenanceTypes;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> LoadCriticalitiesAsync(CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return Criticalities;
    }

    private static (string Name, string Schedule) Normalize(string? name, string? schedule)
    {
        var trimmedName = name?.Trim() ?? "";
        var trimmedSchedule = schedule?.Trim() ?? "";

        if (trimmedName.Length == 0 || trimmedSchedule.Length == 0 || !ScheduleFormat.IsMatch(trimmedSchedule))
        {
            throw Error(HttpStatusCode.BadRequest, "Name and schedule are required, and the schedule must be HH:mm-HH:mm.");
        }

        return (trimmedName, trimmedSchedule);
    }

    private void EnsureNoDuplicates(string name, string schedule, Guid? ignoreId)
    {
        var duplicated = _shifts.Any(s =>
            s.Id != ignoreId
            && (string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase) || s.Schedule == schedule));

        if (duplicated)
        {
            throw Error(HttpStatusCode.Conflict, "A shift with this name or schedule already exists.");
        }
    }

    private static HttpRequestException Error(HttpStatusCode status, string message) =>
        new(message, inner: null, statusCode: status);
}