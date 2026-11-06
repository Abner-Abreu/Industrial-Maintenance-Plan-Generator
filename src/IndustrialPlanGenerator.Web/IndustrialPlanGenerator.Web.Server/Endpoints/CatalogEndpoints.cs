using IndustrialPlanGenerator.Services.Catalogs;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Endpoints de catálogos (CAT-1 a CAT-6).</summary>
public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalogs").WithTags("Catalogs");

        group.MapGet("/shifts", ListShifts)
            .RequireAuthorization()
            .WithName("ListShifts")
            .WithSummary("Lista los turnos (CAT-1).")
            .Produces<IReadOnlyList<ShiftDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/shifts", CreateShift)
            .RequireAuthorization(Policies.Administrator)
            .WithName("CreateShift")
            .WithSummary("Crea un turno (CAT-2).")
            .WithDescription("Nombre y horario deben ser únicos.")
            .Produces<ShiftDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/shifts/{id:guid}", UpdateShift)
            .RequireAuthorization(Policies.Administrator)
            .WithName("UpdateShift")
            .WithSummary("Edita un turno (CAT-3).")
            .Produces<ShiftDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/shifts/{id:guid}", DeleteShift)
            .RequireAuthorization(Policies.Administrator)
            .WithName("DeleteShift")
            .WithSummary("Elimina un turno (CAT-4).")
            .WithDescription("409 si tiene operarios.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/maintenance-types", ListMaintenanceTypes)
            .RequireAuthorization()
            .WithName("ListMaintenanceTypes")
            .WithSummary("Tipos de mantenimiento (CAT-5).")
            .WithDescription("Valores fijos del dominio (MaintenanceType).")
            .Produces<IReadOnlyList<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/criticalities", ListCriticalities)
            .RequireAuthorization()
            .WithName("ListCriticalities")
            .WithSummary("Niveles de criticidad (CAT-6).")
            .WithDescription("Valores fijos del dominio (Criticality).")
            .Produces<IReadOnlyList<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<ShiftDto>>> ListShifts(ICatalogService catalogs, CancellationToken ct)
        => TypedResults.Ok(await catalogs.ListShiftsAsync(ct));

    private static async Task<Created<ShiftDto>> CreateShift(
        [FromBody] CreateShiftRequest request, ICatalogService catalogs, CancellationToken ct)
    {
        var created = await catalogs.CreateShiftAsync(request, ct);
        return TypedResults.Created($"/api/catalogs/shifts/{created.Id}", created);
    }

    private static async Task<Ok<ShiftDto>> UpdateShift(
        Guid id, [FromBody] UpdateShiftRequest request, ICatalogService catalogs, CancellationToken ct)
        => TypedResults.Ok(await catalogs.UpdateShiftAsync(id, request, ct));

    private static async Task<NoContent> DeleteShift(Guid id, ICatalogService catalogs, CancellationToken ct)
    {
        await catalogs.DeleteShiftAsync(id, ct);
        return TypedResults.NoContent();
    }

    private static Ok<IReadOnlyList<string>> ListMaintenanceTypes(ICatalogService catalogs)
        => TypedResults.Ok(catalogs.ListMaintenanceTypes());

    private static Ok<IReadOnlyList<string>> ListCriticalities(ICatalogService catalogs)
        => TypedResults.Ok(catalogs.ListCriticalities());
}
