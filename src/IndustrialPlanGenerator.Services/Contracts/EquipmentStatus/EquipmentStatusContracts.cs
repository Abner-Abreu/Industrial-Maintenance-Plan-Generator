namespace IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;

/// <summary>Valores permitidos del estado operativo de un equipo (EST-3). Se guardan como texto.</summary>
public static class EquipmentStates
{
    public const string Operational = nameof(Operational);
    public const string UnderMaintenance = nameof(UnderMaintenance);
    public const string Stopped = nameof(Stopped);
    public const string Faulty = nameof(Faulty);

    /// <summary>Todos los valores válidos, en el orden en que se muestran.</summary>
    public static readonly IReadOnlyList<string> All = [Operational, UnderMaintenance, Stopped, Faulty];
}

/// <summary>Estado operativo de un equipo (EST-1, EST-2, EST-3).</summary>
/// <param name="SystemId">Id del sistema (equipo).</param>
/// <param name="SystemName">Nombre del sistema.</param>
/// <param name="Status">Uno de <see cref="EquipmentStates"/>.</param>
/// <param name="LastUpdated">Fecha (UTC) de la última actualización del estado.</param>
public sealed record EquipmentStatusDto(Guid SystemId, string SystemName, string Status, DateTime LastUpdated);

/// <summary>Nuevo estado de un equipo (EST-3).</summary>
/// <param name="Status">Uno de <see cref="EquipmentStates"/>: Operational, UnderMaintenance, Stopped o Faulty.</param>
public sealed record UpdateEquipmentStatusRequest(string Status);
