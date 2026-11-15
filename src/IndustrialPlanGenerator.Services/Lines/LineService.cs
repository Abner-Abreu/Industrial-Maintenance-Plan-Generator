using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Exceptions;

namespace IndustrialPlanGenerator.Services.Lines;

/// <inheritdoc cref="ILineService"/>
public sealed class LineService(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILineAccessPolicy lineAccess) : ILineService
{
    /// <summary>Maximum length of a line name (column limit).</summary>
    public const int MaxNameLength = 200;

    /// <summary>Maximum length of a productive process (column limit).</summary>
    public const int MaxProcessLength = 500;

    // ======================= LIN-1 =======================

    public async Task<IReadOnlyList<LineSummaryDto>> ListAsync(CancellationToken ct = default)
    {
        var lines = await uow.Line.GetAllAsync(ct);
        var visibleIds = await GetVisibleLineIdsAsync(ct);

        var visible = lines
            .Where(l => visibleIds is null || visibleIds.Contains(l.Id))
            .OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // GetAllAsync no carga los sistemas, así que se cuentan con GetByLineaAsync (una consulta por línea visible).
        var result = new List<LineSummaryDto>(visible.Count);
        foreach (var line in visible)
        {
            var systems = await uow.Equipments.GetByLineaAsync(line.Id, ct);
            result.Add(new LineSummaryDto(line.Id, line.Name, line.ProductiveProcess, systems.Count));
        }

        return result;
    }

    // ======================= LIN-2 =======================

    public async Task<LineDetailDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        // El acceso se comprueba antes de buscar, para que quien no tiene permiso no pueda averiguar qué ids existen.
        if (!await lineAccess.CanViewAsync(id, ct))
            throw new ForbiddenException();

        var line = await uow.Line.GetWithEquipmentAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(ProductionLine), id);

        return ToDetail(line);
    }

    // ======================= LIN-3 =======================

    public async Task<LineDetailDto> CreateAsync(CreateLineRequest request, CancellationToken ct = default)
    {
        var (name, process) = Validate(request.Name, request.ProductiveProcess);

        var line = new ProductionLine
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            ProductiveProcess = process,
            EquipmentSystems = [],
            TechnicianLines = [],
            Operators = [],
            MaintenancePlans = []
        };

        await uow.Line.AddAsync(line, ct);
        await uow.SaveChangesAsync(ct);
        return ToDetail(line);
    }

    // ======================= LIN-4 =======================

    public async Task<LineDetailDto> UpdateAsync(Guid id, UpdateLineRequest request, CancellationToken ct = default)
    {
        var (name, process) = Validate(request.Name, request.ProductiveProcess);

        var line = await uow.Line.GetWithEquipmentAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(ProductionLine), id);

        line.Name = name;
        line.ProductiveProcess = process;
        uow.Line.Update(line);
        await uow.SaveChangesAsync(ct);
        return ToDetail(line);
    }

    // ======================= LIN-5 =======================

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var line = await uow.Line.GetWithEquipmentAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(ProductionLine), id);

        if (line.EquipmentSystems.Count > 0)
            throw new ConflictException("No se puede eliminar la línea porque tiene sistemas.");
        if ((await uow.Plans.GetByLineAsync(id, ct)).Count > 0)
            throw new ConflictException("No se puede eliminar la línea porque tiene planes de mantenimiento.");
        if ((await uow.Technicians.GetByLineAsync(id, ct)).Count > 0
            || (await uow.Operators.GetByLineAsync(id, ct)).Count > 0)
            throw new ConflictException("No se puede eliminar la línea porque tiene técnicos u operarios asignados.");

        uow.Line.Remove(line);
        await uow.SaveChangesAsync(ct);
    }

    // ======================= Auxiliares =======================

    // null = todas las líneas (administrador y jefe de planta).
    private async Task<HashSet<Guid>?> GetVisibleLineIdsAsync(CancellationToken ct)
    {
        switch (currentUser.Role)
        {
            case UserRole.Administrator:
            case UserRole.PlantManager:
                return null;

            case UserRole.Technician:
                var technician = await uow.Technicians.GetWithLineAsync(currentUser.Id, ct);
                return technician?.Lines.Select(l => l.LineId).ToHashSet() ?? [];

            case UserRole.Operator:
                var op = await uow.Operators.GetWithShiftAndLinesAsync(currentUser.Id, ct);
                return op?.Lines.Select(l => l.LineId).ToHashSet() ?? [];

            default:
                return [];
        }
    }

    private static (string Name, string Process) Validate(string? name, string? process)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidRequestException("El nombre de la línea es obligatorio.");
        if (string.IsNullOrWhiteSpace(process))
            throw new InvalidRequestException("El proceso productivo es obligatorio.");

        name = name.Trim();
        process = process.Trim();

        if (name.Length > MaxNameLength)
            throw new InvalidRequestException($"El nombre de la línea no puede superar {MaxNameLength} caracteres.");
        if (process.Length > MaxProcessLength)
            throw new InvalidRequestException($"El proceso productivo no puede superar {MaxProcessLength} caracteres.");

        return (name, process);
    }

    private static LineDetailDto ToDetail(ProductionLine line) => new(
        line.Id,
        line.Name,
        line.ProductiveProcess,
        line.EquipmentSystems
            .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .Select(s => new EquipmentSystemDto(s.Id, s.Name, s.Description, s.OperativeState))
            .ToList());
}
