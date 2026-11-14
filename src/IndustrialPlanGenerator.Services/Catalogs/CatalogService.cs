using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Services.Exceptions;

namespace IndustrialPlanGenerator.Services.Catalogs;

/// <inheritdoc cref="ICatalogService"/>
public sealed class CatalogService(IUnitOfWork uow) : ICatalogService
{
    /// <summary>Maximum length of a shift name (column limit).</summary>
    public const int MaxShiftNameLength = 100;

    /// <summary>Maximum length of a shift schedule (column limit).</summary>
    public const int MaxShiftScheduleLength = 200;

    // ======================= CAT-1 =======================

    public async Task<IReadOnlyList<ShiftDto>> ListShiftsAsync(CancellationToken ct = default)
    {
        var shifts = await uow.Shifts.GetAllAsync(ct);
        return shifts
            .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .Select(ToDto)
            .ToList();
    }

    // ======================= CAT-2 =======================

    public async Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request, CancellationToken ct = default)
    {
        var (name, schedule) = Validate(request.Name, request.Schedule);
        await EnsureUniqueAsync(name, schedule, exceptId: null, ct);

        var shift = new Shift
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Schedule = schedule,
            Operators = []
        };

        await uow.Shifts.AddAsync(shift, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(shift);
    }

    // ======================= CAT-3 =======================

    public async Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftRequest request, CancellationToken ct = default)
    {
        var (name, schedule) = Validate(request.Name, request.Schedule);

        var shift = await uow.Shifts.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(Shift), id);

        await EnsureUniqueAsync(name, schedule, exceptId: id, ct);

        shift.Name = name;
        shift.Schedule = schedule;
        uow.Shifts.Update(shift);
        await uow.SaveChangesAsync(ct);
        return ToDto(shift);
    }

    // ======================= CAT-4 =======================

    public async Task DeleteShiftAsync(Guid id, CancellationToken ct = default)
    {
        var shift = await uow.Shifts.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(Shift), id);

        // Requiere que el repositorio cargue shift.Operators (Include); si llega vacía, no se detecta el conflicto.
        if (shift.Operators.Count > 0)
            throw new ConflictException("No se puede eliminar el turno porque tiene operarios asignados.");

        uow.Shifts.Remove(shift);
        await uow.SaveChangesAsync(ct);
    }

    // ======================= CAT-5 y CAT-6 =======================

    public IReadOnlyList<string> ListMaintenanceTypes() => Enum.GetNames<MaintenanceType>();

    public IReadOnlyList<string> ListCriticalities() => Enum.GetNames<Criticality>();

    // ======================= Auxiliares =======================

    private static (string Name, string Schedule) Validate(string? name, string? schedule)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidRequestException("El nombre del turno es obligatorio.");
        if (string.IsNullOrWhiteSpace(schedule))
            throw new InvalidRequestException("El horario del turno es obligatorio.");

        name = name.Trim();
        schedule = schedule.Trim();

        if (name.Length > MaxShiftNameLength)
            throw new InvalidRequestException($"El nombre del turno no puede superar {MaxShiftNameLength} caracteres.");
        if (schedule.Length > MaxShiftScheduleLength)
            throw new InvalidRequestException($"El horario del turno no puede superar {MaxShiftScheduleLength} caracteres.");

        return (name, schedule);
    }

    // Nombre y horario son únicos (RI-28). Se comparan sin distinguir mayúsculas, algo más estricto que el
    // índice de la base de datos, para devolver un 409 claro en lugar de dejar escapar su excepción.
    private async Task EnsureUniqueAsync(string name, string schedule, Guid? exceptId, CancellationToken ct)
    {
        var others = (await uow.Shifts.GetAllAsync(ct)).Where(s => s.Id != exceptId).ToList();

        if (others.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"Ya existe un turno con el nombre '{name}'.");
        if (others.Any(s => string.Equals(s.Schedule, schedule, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"Ya existe un turno con el horario '{schedule}'.");
    }

    private static ShiftDto ToDto(Shift shift) => new(shift.Id, shift.Name, shift.Schedule);
}
