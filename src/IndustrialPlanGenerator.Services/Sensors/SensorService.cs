using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Sensors;
using IndustrialPlanGenerator.Services.Exceptions;

namespace IndustrialPlanGenerator.Services.Sensors;

/// <inheritdoc cref="ISensorService"/>
public sealed class SensorService(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILineAccessPolicy lineAccess) : ISensorService
{
    // ======================= SEN-1 =======================

    public async Task<IReadOnlyList<SensorDto>> ListBySystemAsync(Guid systemId, CancellationToken ct = default)
    {
        // Los operarios no consultan sensores (SEN-1: administrador, jefe de planta y técnico).
        if (currentUser.Role == UserRole.Operator)
            throw new ForbiddenException();

        var system = await uow.Equipments.GetByIdAsync(systemId, ct)
            ?? throw new EntityNotFoundException(typeof(EquipmentSystem), systemId);

        // El técnico solo ve los sistemas de sus líneas; administrador y jefe de planta, todos.
        if (!await lineAccess.CanViewAsync(system.LineId, ct))
            throw new ForbiddenException();

        // OJO (R2): GetByIdAsync no hace Include de Sensors, así que hoy esta colección llega vacía y la lista sale vacía.
        // Hace falta añadir el Include en EquipmentSystemRepository.GetByIdAsync.
        return system.Sensors
            .OrderBy(s => s.Type)
            .ThenBy(s => s.Id)
            .Select(s => ToDto(s))
            .ToList();
    }

    // ======================= SEN-2 =======================

    public async Task<SensorDto> CreateAsync(Guid systemId, CreateSensorRequest request, CancellationToken ct = default)
    {
        var type = ParseType(request.Type);

        var system = await uow.Equipments.GetByIdAsync(systemId, ct)
            ?? throw new EntityNotFoundException(typeof(EquipmentSystem), systemId);

        // Id vacío a propósito: el sistema ya está rastreado por EF, y un sensor nuevo con la clave Guid ya asignada
        // se marcaría como Modified (UPDATE de una fila que no existe). Con Guid.Empty, EF lo marca como Added y
        // genera la clave al guardar. Por eso se añade a la colección y no hace falta un método nuevo en el repositorio.
        var sensor = new Sensor
        {
            Id = Guid.Empty,
            EquipmentId = system.Id,
            Type = type,
            Equipment = system
        };

        system.Sensors.Add(sensor);
        await uow.SaveChangesAsync(ct);
        return ToDto(sensor);
    }

    // ======================= Auxiliares =======================

    // Solo se aceptan los nombres del enum (sin distinguir mayúsculas); los números ("0", "1") se rechazan,
    // porque Enum.TryParse los daría por válidos.
    private static SensorType ParseType(string? type)
    {
        var valid = string.Join(", ", Enum.GetNames<SensorType>());

        if (string.IsNullOrWhiteSpace(type))
            throw new InvalidRequestException($"El tipo de sensor es obligatorio. Valores válidos: {valid}.");

        var text = type.Trim();
        if (int.TryParse(text, out _) || !Enum.TryParse<SensorType>(text, ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
            throw new InvalidRequestException($"Tipo de sensor no válido: '{text}'. Valores válidos: {valid}.");

        return parsed;
    }

    private static SensorDto ToDto(Sensor s) => new(s.Id, s.EquipmentId, s.Type.ToString());
}
