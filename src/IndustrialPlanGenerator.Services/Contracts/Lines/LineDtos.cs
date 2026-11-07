namespace IndustrialPlanGenerator.Services.Contracts.Lines;

/// <summary>Production line in a listing (LIN-1).</summary>
/// <param name="Id">Line ID.</param>
/// <param name="Name">Name.</param>
/// <param name="ProductiveProcess">Productive process performed.</param>
/// <param name="SystemCount">Number of systems (equipment) it has.</param>
public sealed record LineSummaryDto(Guid Id, string Name, string ProductiveProcess, int SystemCount);

/// <summary>System or equipment of a line. It is reused by the system endpoints (SIS-*).</summary>
/// <param name="Id">System ID.</param>
/// <param name="Name">Name.</param>
/// <param name="Description">Description.</param>
/// <param name="OperativeState">Current operative state (managed by the telemetry service, not edited here).</param>
public sealed record EquipmentSystemDto(Guid Id, string Name, string Description, string OperativeState);

/// <summary>Line details including its systems (LIN-2, LIN-3, LIN-4).</summary>
public sealed record LineDetailDto(
    Guid Id,
    string Name,
    string ProductiveProcess,
    IReadOnlyList<EquipmentSystemDto> Systems);
