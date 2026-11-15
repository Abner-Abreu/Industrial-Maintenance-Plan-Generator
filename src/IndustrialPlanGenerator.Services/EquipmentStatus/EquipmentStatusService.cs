using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using CachedStatus = IndustrialPlanGenerator.Domain.Telemetry.EquipmentStatus;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;
using IndustrialPlanGenerator.Services.Exceptions;
using Microsoft.Extensions.Logging;

namespace IndustrialPlanGenerator.Services.EquipmentStatus;

/// <inheritdoc cref="IEquipmentStatusService"/>
/// <remarks>
/// PostgreSQL es la fuente de verdad (<c>EquipmentSystem.OperativeState</c>); la caché de MongoDB solo acelera las
/// lecturas. Si MongoDB falla, el servicio sigue funcionando con PostgreSQL y lo registra como aviso.
/// </remarks>
public sealed class EquipmentStatusService(
    IUnitOfWork uow,
    IEquipmentStatusCache cache,
    ICurrentUser currentUser,
    ILineAccessPolicy lineAccess,
    TimeProvider time,
    ILogger<EquipmentStatusService> logger) : IEquipmentStatusService
{
    // ======================= EST-1 =======================

    public async Task<EquipmentStatusDto> GetAsync(Guid systemId, CancellationToken ct = default)
    {
        var system = await uow.Equipments.GetByIdAsync(systemId, ct)
            ?? throw new EntityNotFoundException(typeof(EquipmentSystem), systemId);

        if (!await lineAccess.CanViewAsync(system.LineId, ct))
            throw new ForbiddenException();

        return await ResolveAsync(system, new CacheAvailability(), ct);
    }

    // ======================= EST-2 =======================

    public async Task<IReadOnlyList<EquipmentStatusDto>> ListByLineAsync(Guid lineId, CancellationToken ct = default)
    {
        // El acceso se comprueba antes de buscar, para que quien no tiene permiso no pueda averiguar qué ids existen.
        if (!await lineAccess.CanViewAsync(lineId, ct))
            throw new ForbiddenException();

        _ = await uow.Line.GetByIdAsync(lineId, ct)
            ?? throw new EntityNotFoundException(typeof(ProductionLine), lineId);

        var systems = await uow.Equipments.GetByLineaAsync(lineId, ct);

        // Si la caché falla una vez, no se vuelve a intentar con el resto de equipos de esta petición
        // (evita esperar un tiempo de espera de MongoDB por cada equipo).
        var availability = new CacheAvailability();
        var result = new List<EquipmentStatusDto>(systems.Count);
        foreach (var system in systems.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
            result.Add(await ResolveAsync(system, availability, ct));

        return result;
    }

    // ======================= EST-3 =======================

    public async Task<EquipmentStatusDto> UpdateAsync(
        Guid systemId, UpdateEquipmentStatusRequest request, CancellationToken ct = default)
    {
        // Solo administrador y técnico cambian el estado.
        if (currentUser.Role is not (UserRole.Administrator or UserRole.Technician))
            throw new ForbiddenException();

        var status = ParseStatus(request.Status);

        var system = await uow.Equipments.GetByIdAsync(systemId, ct)
            ?? throw new EntityNotFoundException(typeof(EquipmentSystem), systemId);

        // El técnico solo en equipos de sus líneas; el administrador en cualquiera.
        if (currentUser.Role == UserRole.Technician && !await lineAccess.CanViewAsync(system.LineId, ct))
            throw new ForbiddenException();

        // 1) PostgreSQL primero: es la fuente de verdad.
        system.OperativeState = status;
        uow.Equipments.Update(system);
        await uow.SaveChangesAsync(ct);

        // 2) Después la caché, sin que un fallo de MongoDB deshaga ni rompa el cambio ya guardado.
        var now = time.GetUtcNow().UtcDateTime;
        await RefreshCacheAfterUpdateAsync(system.Id, status, now, ct);

        return ToDto(system, status, now);
    }

    // ======================= Auxiliares =======================

    private async Task<EquipmentStatusDto> ResolveAsync(
        EquipmentSystem system, CacheAvailability availability, CancellationToken ct)
    {
        var cached = await TryGetCachedAsync(system.Id, availability, ct);
        if (cached is not null && EquipmentStates.All.Contains(cached.OperatingStatus))
            return ToDto(system, cached.OperatingStatus, cached.LastUpdated);

        // Sin caché válida: se responde con lo que dice PostgreSQL y se intenta rellenar la caché.
        // PostgreSQL no guarda la fecha del último cambio, así que se usa la de ahora.
        var now = time.GetUtcNow().UtcDateTime;
        await TryUpsertAsync(system.Id, system.OperativeState, now, availability, ct);
        return ToDto(system, system.OperativeState, now);
    }

    private async Task<CachedStatus?> TryGetCachedAsync(
        Guid systemId, CacheAvailability availability, CancellationToken ct)
    {
        if (!availability.IsAvailable)
            return null;

        try
        {
            return await cache.GetAsync(systemId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            availability.IsAvailable = false;
            logger.LogWarning(ex, "No se pudo leer la caché de estado del equipo {SystemId}; se usa PostgreSQL.", systemId);
            return null;
        }
    }

    private async Task TryUpsertAsync(
        Guid systemId, string status, DateTime now, CacheAvailability availability, CancellationToken ct)
    {
        if (!availability.IsAvailable)
            return;

        try
        {
            await cache.UpsertAsync(NewCacheEntry(systemId, status, now), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            availability.IsAvailable = false;
            logger.LogWarning(ex, "No se pudo rellenar la caché de estado del equipo {SystemId}.", systemId);
        }
    }

    private async Task RefreshCacheAfterUpdateAsync(Guid systemId, string status, DateTime now, CancellationToken ct)
    {
        try
        {
            await cache.UpsertAsync(NewCacheEntry(systemId, status, now), ct);
            return;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "No se pudo actualizar la caché de estado del equipo {SystemId}; se intenta invalidarla.", systemId);
        }

        // Si la caché no se pudo actualizar, hay que quitar el valor viejo para que no se sirva como actual.
        try
        {
            await cache.InvalidateAsync(systemId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "No se pudo invalidar la caché de estado del equipo {SystemId}: puede contener un valor desactualizado.", systemId);
        }
    }

    private static CachedStatus NewCacheEntry(Guid systemId, string status, DateTime now) => new()
    {
        EquipmentId = systemId,
        OperatingStatus = status,
        LastUpdated = now
    };

    // Devuelve el nombre canónico del estado, sin distinguir mayúsculas. Los números no valen.
    private static string ParseStatus(string? status)
    {
        var valid = string.Join(", ", EquipmentStates.All);

        if (string.IsNullOrWhiteSpace(status))
            throw new InvalidRequestException($"El estado es obligatorio. Valores válidos: {valid}.");

        var text = status.Trim();
        var canonical = EquipmentStates.All.FirstOrDefault(s => string.Equals(s, text, StringComparison.OrdinalIgnoreCase));
        return canonical
            ?? throw new InvalidRequestException($"Estado no válido: '{text}'. Valores válidos: {valid}.");
    }

    private static EquipmentStatusDto ToDto(EquipmentSystem system, string status, DateTime lastUpdated) =>
        new(system.Id, system.Name, status, lastUpdated);

    // Recuerda, durante una petición, si la caché ha fallado.
    private sealed class CacheAvailability
    {
        public bool IsAvailable { get; set; } = true;
    }
}
