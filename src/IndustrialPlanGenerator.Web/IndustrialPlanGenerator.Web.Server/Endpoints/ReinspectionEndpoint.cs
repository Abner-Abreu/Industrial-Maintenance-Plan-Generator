using Microsoft.AspNetCore.Http.HttpResults;
using IndustrialPlanGenerator.Web.Server.Auth;
using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.Validations;
using IndustrialPlanGenerator.Services.Contracts.Reinspection;


public static class ReinspectionEndpoints
{
    public static IEndpointRouteBuilder MapReinspectionEndpoints (this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var reinspecciones = api.MapGroup("/reinspecciones");

        // REI-1: POST /api/reinspecciones
        reinspecciones.MapPost("/", CreateReinspection)
        .WithName("CreateReinspection")
        .WithSummary("Generar una reinspeccion")
        .Produces<ReinspectionDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Operator));
        
        // REI-2: GET /api/reinspecciones [PDF]
        reinspecciones.MapGet("/", GetReinspectionsFiltered)
        .WithName("GetReinspection")
        .WithSummary("Obtener una reinspeccion")
        .Produces<ReinspectionListDto>(StatusCodes.Status200OK)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Operator, Policies.Technician, Policies.PlantManager, Policies.Administrator));

        // REI-3: GET /api/reinspecciones/{id}
        reinspecciones.MapGet("/{id:guid}", GetReinspectionsbyId)
        .WithName("GetReinspectionbyId")
        .WithSummary("Obtener una reinspeccion a traves de un id")
        .Produces<ReinspectionDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Operator, Policies.Technician, Policies.PlantManager, Policies.Administrator));
        
        // REI-4: PUT /api/reinspecciones/{id}/resultado        Cambio de Post->Put
        reinspecciones.MapPost("/{id:guid}/resultado", PutManualResult)
        .WithName("CreateResults")
        .WithSummary("Crear resultados de una reinspeccion")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician));
        
        return app;
    }


    /// <summary>
    /// Llama al servicio para crear una reinspeccion
    /// </summary>
    /// <param name="user"> Usuario</param>
    /// <param name="reinspecciondata"> Informacion de la Reinspeccion</param>
    /// <param name="service"> Servicio de Reinspeccion</param>
    /// <returns> Retorna la reinspeccion en caso de ser exitosa, y arroja unprocessable si no cumple los criterios</returns>
    private static async Task<Results<Created<ReinspectionDto>, UnprocessableEntity>> CreateReinspection(ClaimsPrincipal user, ReinspectionCreateDto reinspectiondata, IReinspectionService service)
    {
        var (result, myexec, techAuthorize) = await service.GenerateReinspection(reinspectiondata, user);

        if (!myexec || !techAuthorize)
        {
            TypedResults.UnprocessableEntity();
        }

        return TypedResults.Created("", result);
    }


    /// <summary>
    /// Obtener Reinspecciones por rol
    /// </summary>
    /// <param name="user"> Usuario</param>
    /// <param name="filter"> filtro</param>
    /// <param name="service"> servicio de Reinspecciones</param>
    /// <returns></returns>
    private static async Task<Ok<ReinspectionListDto>> GetReinspectionsFiltered(ClaimsPrincipal user, ReinspectionFilterDto filter, IReinspectionService service)
    {
        ReinspectionListDto result = await service.GetReinspections(user, filter);
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Obtener Reinspecciones por rol a traves de in id
    /// </summary>
    /// <param name="id"> id de la reinspeccion</param>
    /// <param name="user"> Usuario</param>
    /// <param name="service"> servicio de Reinspecciones</param>
    /// <returns></returns>
    private static async Task<Results<Ok<ReinspectionDto>, NotFound>> GetReinspectionsbyId(Guid id, ClaimsPrincipal user, IReinspectionService service)
    {
        var (result, notFound) = await service.GetReinspectionsbyId(id, user);
        if (notFound)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Insertar respuesta de reinspeccion en la solicitud
    /// </summary>
    /// <param name="id"> id de la solicitud (por URL)</param>
    /// <param name="user"> usuario</param>
    /// <param name="result"> resultado manual</param>
    /// <param name="service"> servicio de Reinspeccion</param>
    /// <returns> Respuestas http de Ok y Unauthorized</returns>
    private static async Task<Results<Ok, UnauthorizedHttpResult>> PutManualResult(Guid id, ClaimsPrincipal user, ReinspectionResultDto result, IReinspectionService service)
    {
        bool Unauthorized = await service.PutReinspectionResult(id, user, result);
        if (Unauthorized)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok();
    }


}