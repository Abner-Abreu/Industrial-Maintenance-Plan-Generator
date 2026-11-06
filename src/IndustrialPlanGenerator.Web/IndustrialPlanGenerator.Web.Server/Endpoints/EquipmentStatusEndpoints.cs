using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;
using IndustrialPlanGenerator.Services.EquipmentStatus;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Endpoints del estado operativo de equipos (EST-1 a EST-3).</summary>
public static class EquipmentStatusEndpoints
{
    public static IEndpointRouteBuilder MapEquipmentStatusEndpoints(this IEndpointRouteBuilder app)
    {
        var systems = app.MapGroup("/api/systems/{systemId:guid}/status").WithTags("Equipment status");

        systems.MapGet("/", Get)
            .RequireAuthorization()
            .WithName("GetEquipmentStatus")
            .WithSummary("Estado operativo de un equipo (EST-1).")
            .WithDescription("Requiere acceso a la línea del equipo (incluye operarios). Se lee de la caché.")
            .Produces<EquipmentStatusDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        systems.MapPut("/", Update)
            .RequireAuthorization(p => p.RequireRole(Policies.Administrator, Policies.Technician))
            .WithName("UpdateEquipmentStatus")
            .WithSummary("Cambia el estado operativo de un equipo (EST-3).")
            .WithDescription("Valores: Operational, UnderMaintenance, Stopped, Faulty. El técnico solo en equipos de sus líneas.")
            .Produces<EquipmentStatusDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // EST-2 cuelga de /api/lines, pero es un endpoint de estado.
        app.MapGet("/api/lines/{lineId:guid}/systems-status", ListByLine)
            .WithTags("Equipment status")
            .RequireAuthorization()
            .WithName("ListLineEquipmentStatus")
            .WithSummary("Estado de todos los equipos de una línea (EST-2).")
            .WithDescription("Requiere acceso a la línea. Es el tablero del operario.")
            .Produces<IReadOnlyList<EquipmentStatusDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Ok<EquipmentStatusDto>> Get(
        Guid systemId, IEquipmentStatusService status, CancellationToken ct)
        => TypedResults.Ok(await status.GetAsync(systemId, ct));

    private static async Task<Ok<EquipmentStatusDto>> Update(
        Guid systemId, [FromBody] UpdateEquipmentStatusRequest request, IEquipmentStatusService status, CancellationToken ct)
        => TypedResults.Ok(await status.UpdateAsync(systemId, request, ct));

    private static async Task<Ok<IReadOnlyList<EquipmentStatusDto>>> ListByLine(
        Guid lineId, IEquipmentStatusService status, CancellationToken ct)
        => TypedResults.Ok(await status.ListByLineAsync(lineId, ct));
}
