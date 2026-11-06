using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Contracts.Systems;
using IndustrialPlanGenerator.Services.Systems;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Endpoints de sistemas o equipos (SIS-1 a SIS-4).</summary>
public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        // SIS-1 cuelga de /api/lines, pero es un endpoint de sistemas.
        app.MapGet("/api/lines/{lineId:guid}/systems", ListByLine)
            .WithTags("Systems")
            .RequireAuthorization()
            .WithName("ListLineSystems")
            .WithSummary("Sistemas de una línea (SIS-1).")
            .WithDescription("Requiere acceso a la línea.")
            .Produces<IReadOnlyList<EquipmentSystemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        var group = app.MapGroup("/api/systems").WithTags("Systems");

        group.MapPost("/", Create)
            .RequireAuthorization(Policies.Administrator)
            .WithName("CreateSystem")
            .WithSummary("Crea un sistema en una línea (SIS-2).")
            .WithDescription("El estado operativo inicial lo asigna el servidor.")
            .Produces<EquipmentSystemDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", Update)
            .RequireAuthorization(Policies.Administrator)
            .WithName("UpdateSystem")
            .WithSummary("Edita nombre y descripción de un sistema (SIS-3).")
            .WithDescription("No cambia el estado operativo ni la línea.")
            .Produces<EquipmentSystemDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .RequireAuthorization(Policies.Administrator)
            .WithName("DeleteSystem")
            .WithSummary("Elimina un sistema (SIS-4).")
            .WithDescription("409 si tiene tareas o sensores.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<EquipmentSystemDto>>> ListByLine(
        Guid lineId, IEquipmentSystemService systems, CancellationToken ct)
        => TypedResults.Ok(await systems.ListByLineAsync(lineId, ct));

    private static async Task<Created<EquipmentSystemDto>> Create(
        [FromBody] CreateSystemRequest request, IEquipmentSystemService systems, CancellationToken ct)
    {
        var created = await systems.CreateAsync(request, ct);
        return TypedResults.Created($"/api/systems/{created.Id}", created);
    }

    private static async Task<Ok<EquipmentSystemDto>> Update(
        Guid id, [FromBody] UpdateSystemRequest request, IEquipmentSystemService systems, CancellationToken ct)
        => TypedResults.Ok(await systems.UpdateAsync(id, request, ct));

    private static async Task<NoContent> Delete(Guid id, IEquipmentSystemService systems, CancellationToken ct)
    {
        await systems.DeleteAsync(id, ct);
        return TypedResults.NoContent();
    }
}
