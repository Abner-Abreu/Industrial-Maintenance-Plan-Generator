namespace IndustrialPlanGenerator.Services.Contracts.Lines;

/// <summary>Línea de producción en un listado (LIN-1).</summary>
/// <param name="Id">Id de la línea.</param>
/// <param name="Name">Nombre.</param>
/// <param name="ProductiveProcess">Proceso productivo que realiza.</param>
/// <param name="SystemCount">Cantidad de sistemas (equipos) que tiene.</param>
public sealed record LineSummaryDto(Guid Id, string Name, string ProductiveProcess, int SystemCount);

/// <summary>Sistema o equipo de una línea. Lo reutilizan los endpoints de sistemas (SIS-*).</summary>
/// <param name="Id">Id del sistema.</param>
/// <param name="Name">Nombre.</param>
/// <param name="Description">Descripción.</param>
/// <param name="OperativeState">Estado operativo actual (lo gestiona el servicio de telemetría, no se edita aquí).</param>
public sealed record EquipmentSystemDto(Guid Id, string Name, string Description, string OperativeState);

/// <summary>Detalle de una línea con sus sistemas (LIN-2, LIN-3, LIN-4).</summary>
public sealed record LineDetailDto(
    Guid Id,
    string Name,
    string ProductiveProcess,
    IReadOnlyList<EquipmentSystemDto> Systems);
