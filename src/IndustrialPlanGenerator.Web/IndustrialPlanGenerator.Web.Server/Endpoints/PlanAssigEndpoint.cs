using Microsoft.AspNetCore.Http.HttpResults;
using IndustrialPlanGenerator.Web.Server.Auth;
using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.PlanAssig;


public static class PlanAssigEndpoints
{
    public static IEndpointRouteBuilder MapPlanAssigEndpoints (this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var asignaciones = api.MapGroup("/asignaciones");

        // ASG-1: POST /api/asignaciones
        asignaciones.MapPost("/", CreateAssig)
        .WithName("CreateAssig")
        .WithSummary("Crear una asignacion")
        .Produces<AssigGetOne>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Administrator, Policies.PlantManager));
        
        // ASG-2: DELETE /api/asignaciones/{id}/cancelar
        asignaciones.MapPost("/{id:guid}/cancelar", CancelAssig)
        .WithName("CreateAssig")
        .WithSummary("Cancelar una asignacion")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Administrator, Policies.PlantManager));
        
        // ASG-3: GET /api/asignaciones [PDF]
        asignaciones.MapGet("/", GetAssig)
        .WithName("GetAssig")
        .WithSummary("Obtener Asignaciones por filtro")
        .Produces<List<AssigGetOne>>(StatusCodes.Status200OK)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Administrator, Policies.PlantManager, Policies.Technician));
        
        return app;
    }

    /// <summary>
    /// Datos para crear una asignacion
    /// </summary>
    /// <param name="assig">Datos de la asignacion</param>
    /// <param name="service">Servicio de Asignacion</param>
    /// <returns> Asignacion nueva</returns>
    private static async Task<Results<Created<AssigGetOne>, UnprocessableEntity, Conflict>> CreateAssig(AssignationCreateDto assig, IAssigPlans service)
    {
        var (result, IsValid, belongline, IsDouble) = await service.CreateAssig(assig);

        if (!IsValid || !belongline)
        {
            return TypedResults.UnprocessableEntity();
        }
        else if (IsDouble)
        {
            return TypedResults.Conflict();
        }
        else
        {
            return TypedResults.Created("", result);
        }
    }


    /// <summary>
    /// Cancela una asignacion
    /// </summary>
    /// <param name="id"> id de la asignacion</param>
    /// <param name="service"> Servicio de asignacion</param>
    /// <returns> Llamada http vacia con confirmacion o no encontrada</returns>
    private static async Task<Results<Ok,NotFound>> CancelAssig(Guid id, IAssigPlans service)
    {
        bool notFoundAssig = await service.CancelAssig(id);
        if (notFoundAssig)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok();
    }


    /// <summary>
    /// Obtener asignaciones con valores de filtrado
    /// </summary>
    /// <param name="user"> usuario</param>
    /// <param name="filter"> filtro de las asignaciones</param>
    /// <param name="service"> Servicio de asignacion</param>
    /// <returns> asignaciones filtradas</returns>
    private static async Task<Ok<List<AssigGetOne>>> GetAssig(ClaimsPrincipal user, AssignationFilterDto filter, IAssigPlans service)
    {
        List<AssigGetOne> result = await service.GetAssig(user, filter);
        return TypedResults.Ok(result);
    }

}