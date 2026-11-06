namespace IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;

/// <summary>Allowed values for the operational state of an equipment (EST-3). They are stored as text.</summary>
public static class EquipmentStates
{
    public const string Operational = nameof(Operational);
    public const string UnderMaintenance = nameof(UnderMaintenance);
    public const string Stopped = nameof(Stopped);
    public const string Faulty = nameof(Faulty);

    /// <summary>All valid values, in display order.</summary>
    public static readonly IReadOnlyList<string> All = [Operational, UnderMaintenance, Stopped, Faulty];
}

/// <summary>Operational state of an equipment (EST-1, EST-2, EST-3).</summary>
/// <param name="SystemId">System (equipment) ID.</param>
/// <param name="SystemName">System name.</param>
/// <param name="Status">One of <see cref="EquipmentStates"/>.</param>
/// <param name="LastUpdated">Date (UTC) of the last state update.</param>
public sealed record EquipmentStatusDto(Guid SystemId, string SystemName, string Status, DateTime LastUpdated);

/// <summary>New state of an equipment (EST-3).</summary>
/// <param name="Status">One of <see cref="EquipmentStates"/>: Operational, UnderMaintenance, Stopped or Faulty.</param>
public sealed record UpdateEquipmentStatusRequest(string Status);
