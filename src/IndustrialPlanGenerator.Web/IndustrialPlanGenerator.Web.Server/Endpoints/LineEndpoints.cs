using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Lines;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Endpoints de líneas de producción (LIN-1 a LIN-5).</summary>
public static class LineEndpoints
{
    public static IEndpointRouteBuilder MapLineEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lines").WithTags("Lines");

        group.MapGet("/", List)
            .RequireAuthorization()
            .WithName("ListLines")
            .WithSummary("Lista las líneas visibles para el usuario (LIN-1).")
            .WithDescription("Administrador y jefe de planta ven todas; técnicos y operarios, solo las suyas.")
            .Produces<IReadOnlyList<LineSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", Get)
            .RequireAuthorization()
            .WithName("GetLine")
            .WithSummary("Detalle de una línea con sus sistemas (LIN-2).")
            .WithDescription("Requiere acceso a la línea.")
            .Produces<LineDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .RequireAuthorization(Policies.Administrator)
            .WithName("CreateLine")
            .WithSummary("Crea una línea de producción (LIN-3).")
            .Produces<LineDetailDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", Update)
            .RequireAuthorization(Policies.Administrator)
            .WithName("UpdateLine")
            .WithSummary("Edita nombre y proceso productivo de una línea (LIN-4).")
            .Produces<LineDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .RequireAuthorization(Policies.Administrator)
            .WithName("DeleteLine")
            .WithSummary("Elimina una línea (LIN-5).")
            .WithDescription("409 si la línea tiene sistemas, planes o asignaciones.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<LineSummaryDto>>> List(ILineService lines, CancellationToken ct)
        => TypedResults.Ok(await lines.ListAsync(ct));

    private static async Task<Ok<LineDetailDto>> Get(Guid id, ILineService lines, CancellationToken ct)
        => TypedResults.Ok(await lines.GetAsync(id, ct));

    private static async Task<Created<LineDetailDto>> Create(
        [FromBody] CreateLineRequest request, ILineService lines, CancellationToken ct)
    {
        var created = await lines.CreateAsync(request, ct);
        return TypedResults.Created($"/api/lines/{created.Id}", created);
    }

    private static async Task<Ok<LineDetailDto>> Update(
        Guid id, [FromBody] UpdateLineRequest request, ILineService lines, CancellationToken ct)
        => TypedResults.Ok(await lines.UpdateAsync(id, request, ct));

    private static async Task<NoContent> Delete(Guid id, ILineService lines, CancellationToken ct)
    {
        await lines.DeleteAsync(id, ct);
        return TypedResults.NoContent();
    }
}
