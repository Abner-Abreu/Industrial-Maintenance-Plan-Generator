using Microsoft.AspNetCore.Http.HttpResults;
using IndustrialPlanGenerator.Web.Server.Auth;
using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.Validations;


public static class PlanValidationEndpoints
{
    public static IEndpointRouteBuilder MapPlanValidationEndpoints (this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var planes = api.MapGroup("/planes");

        // VAL-1: POST /api/planes/{id}/validaciones
        planes.MapPost("/{id:guid}/validaciones", ValidatePlan)
        .WithName("ValidatePlan")
        .WithSummary("Validar un plan de mantenimiento")
        .Produces<ValidationDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.PlantManager));

        // VAL-2: GET /api/validaciones [PDF]
        api.MapGet("/validaciones", GetValidations)
        .WithName("ValidationFilter")
        .WithSummary("Obtener Validaciones filtradas por parametros")
        .Produces<List<ValidationDto>>(StatusCodes.Status200OK)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.PlantManager, Policies.Administrator));

        return app;
    }

    /// <summary>
    /// Validar un Plan de Mantenimiento
    /// </summary>
    /// <param name="id"> id del plan</param>
    /// <param name="user"> usuario</param>
    /// <param name="validinfo"> informacion de validacion</param>
    /// <param name="service"> Servicio de Validacion</param>
    /// <returns></returns>
    private static async Task<Results<Created<ValidationDto>, UnprocessableEntity>> ValidatePlan(Guid id, ClaimsPrincipal user, ValidCreateDto validinfo, IPlanValidationService service)
    {
        var (result, validateSelf, noGeneratePlan, NonCriteria, NonValidwithoutComments) = await service.ValidatePlan(id, user, validinfo);
        if(validateSelf || noGeneratePlan || NonCriteria || NonValidwithoutComments)
        {
            return TypedResults.UnprocessableEntity();
        }
        return TypedResults.Created("", result);
    }


    /// <summary>
    /// Obtener las validaciones por filtro
    /// </summary>
    /// <param name="filter"> filtro de las validaciones</param>
    /// <param name="service"> Servicio de Validaciones</param>
    /// <returns> Lista de Validaciones</returns>
    private static async Task<Ok<List<ValidationDto>>> GetValidations(ValidFilterDto filter, IPlanValidationService service)
    {
        List<ValidationDto> result = await service.GetValidations(filter);
        return TypedResults.Ok(result);
    }

}