using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;
using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Contracts.Systems;
using IndustrialPlanGenerator.Services.Exceptions;

namespace IndustrialPlanGenerator.Services.Systems;

/// <inheritdoc cref="IEquipmentSystemService"/>
public sealed class EquipmentSystemService(
    IUnitOfWork uow,
    ILineAccessPolicy lineAccess) : IEquipmentSystemService
{
    /// <summary>Maximum length of a system name (column limit).</summary>
    public const int MaxNameLength = 200;

    /// <summary>Maximum length of a system description (column limit).</summary>
    public const int MaxDescriptionLength = 1000;

    // ======================= SIS-1 =======================

    public async Task<IReadOnlyList<EquipmentSystemDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default)
    {
        // El acceso se comprueba antes de buscar, para que quien no tiene permiso no pueda averiguar qué ids existen.
        if (!await lineAccess.CanViewAsync(lineId, ct))
            throw new ForbiddenException();

        _ = await uow.Line.GetByIdAsync(lineId, ct)
            ?? throw new EntityNotFoundException(typeof(ProductionLine), lineId);

        var systems = await uow.Equipments.GetByLineaAsync(lineId, ct);
        return systems
            .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .Select(ToDto)
            .ToList();
    }

    // ======================= SIS-2 =======================

    public async Task<EquipmentSystemDto> CreateAsync(CreateSystemRequest request, CancellationToken ct = default)
    {
        var (name, description) = Validate(request.Name, request.Description);

        var line = await uow.Line.GetByIdAsync(request.LineId, ct)
            ?? throw new EntityNotFoundException(typeof(ProductionLine), request.LineId);

        var system = new EquipmentSystem
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            // El estado inicial lo decide el servidor; luego se cambia con EST-3.
            OperativeState = EquipmentStates.Operational,
            LineId = line.Id,
            ProductionLine = line,
            Sensors = [],
            MaintenanceTasks = []
        };

        await uow.Equipments.AddAsync(system, ct);
        await uow.SaveChangesAsync(ct);
        return ToDto(system);
    }

    // ======================= SIS-3 =======================

    public async Task<EquipmentSystemDto> UpdateAsync(Guid id, UpdateSystemRequest request, CancellationToken ct = default)
    {
        var (name, description) = Validate(request.Name, request.Description);

        var system = await uow.Equipments.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(EquipmentSystem), id);

        // No se toca la línea ni el estado operativo.
        system.Name = name;
        system.Description = description;
        uow.Equipments.Update(system);
        await uow.SaveChangesAsync(ct);
        return ToDto(system);
    }

    // ======================= SIS-4 =======================

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var system = await uow.Equipments.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(EquipmentSystem), id);

        // OJO (R2): EquipmentSystemRepository.GetByIdAsync no hace Include de MaintenanceTasks ni de Sensors,
        // así que hoy estas colecciones llegan vacías y los dos 409 no se disparan. Hace falta añadir ambos Include.
        if (system.MaintenanceTasks.Count > 0)
            throw new ConflictException("No se puede eliminar el sistema porque tiene tareas de mantenimiento.");
        if (system.Sensors.Count > 0)
            throw new ConflictException("No se puede eliminar el sistema porque tiene sensores.");

        uow.Equipments.Remove(system);
        await uow.SaveChangesAsync(ct);
    }

    // ======================= Auxiliares =======================

    private static (string Name, string Description) Validate(string? name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidRequestException("El nombre del sistema es obligatorio.");
        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidRequestException("La descripción del sistema es obligatoria.");

        name = name.Trim();
        description = description.Trim();

        if (name.Length > MaxNameLength)
            throw new InvalidRequestException($"El nombre del sistema no puede superar {MaxNameLength} caracteres.");
        if (description.Length > MaxDescriptionLength)
            throw new InvalidRequestException($"La descripción del sistema no puede superar {MaxDescriptionLength} caracteres.");

        return (name, description);
    }

    private static EquipmentSystemDto ToDto(EquipmentSystem s) => new(s.Id, s.Name, s.Description, s.OperativeState);
}
