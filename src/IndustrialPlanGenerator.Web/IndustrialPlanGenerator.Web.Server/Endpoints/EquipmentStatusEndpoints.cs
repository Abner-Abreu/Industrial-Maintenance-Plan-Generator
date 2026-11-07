using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;
using IndustrialPlanGenerator.Services.EquipmentStatus;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Equipment operational status endpoints (EST-1 to EST-3).</summary>
public static class EquipmentStatusEndpoints
{
    public static IEndpointRouteBuilder MapEquipmentStatusEndpoints(this IEndpointRouteBuilder app)
    {
        var systems = app.MapGroup("/api/systems/{systemId:guid}/status").WithTags("Equipment status");

        systems.MapGet("/", Get)
            .RequireAuthorization()
            .WithName("GetEquipmentStatus")
            .WithSummary("Operational state of an equipment (EST-1).")
            .WithDescription("Requires access to the equipment's line (includes operators). It is read from the cache.")
            .Produces<EquipmentStatusDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        systems.MapPut("/", Update)
            .RequireAuthorization(p => p.RequireRole(Policies.Administrator, Policies.Technician))
            .WithName("UpdateEquipmentStatus")
            .WithSummary("Changes the operational state of an equipment (EST-3).")
            .WithDescription("Values: Operational, UnderMaintenance, Stopped, Faulty. The technician only on equipment of their lines.")
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
            .WithSummary("State of all the equipment of a line (EST-2).")
            .WithDescription("Requires access to the line. It is the operator's dashboard.")
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
