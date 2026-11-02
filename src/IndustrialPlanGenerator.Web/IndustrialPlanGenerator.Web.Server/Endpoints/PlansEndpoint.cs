using Microsoft.AspNetCore.Http.HttpResults;
using IndustrialPlanGenerator.Web.Server.Auth;
using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.Plans;


public static class PlansEndpoints
{
    public static IEndpointRouteBuilder MapPlansEndpoints (this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var planes = api.MapGroup("/planes");
        
        // PLN-1: POST /api/planes/previsualizar
        planes.MapPost("/previsualizar", PreviewPlans)
        .WithName("PrevPlan")
        .WithSummary("Devuelve las tareas y criterios del plan de mantenimiento generado.")
        .Produces<PlanDto>(StatusCodes.Status201Created)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician));

        // PLN-2: POST /api/planes
        planes.MapPost("/", NonCompliancePlan)
        .WithName("NonCompliancePlan")
        .WithSummary("Devuelve las tareas y criterios del plan de mantenimiento generado.")
        .Produces<PlanDto>(StatusCodes.Status201Created)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician));

        // PLN-3: GET /api/planes [PDF]
        planes.MapGet("/", FilterPlans)
        .WithName("NonCompliancePlan")
        .WithSummary("Filtrar planes de mantenimiento con parametros.")
        .Produces<List<PlanDto>>(StatusCodes.Status200OK)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.PlantManager, Policies.Administrator));
        
        // PLN-4: GET /api/planes/{id} [PDF]
        planes.MapGet("/{id:guid}", ViewPlan)
        .WithName("ViewPlan")
        .WithSummary("Ver plan de mantenimiento y sus validaciones.")
        .Produces<PlanViewDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.PlantManager, Policies.Administrator, Policies.Operator));

        // PLN-5: GET /api/planes/{id}/trazabilidad
        planes.MapGet("/{id:guid}/trazabilidad", TrazabilityPlan)
        .WithName("TrazabilityPlan")
        .WithSummary("Historia completa del plan de mantenimiento.")
        .Produces<PlanTrazabilityDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.PlantManager, Policies.Administrator));

        return app;
    }


    /// <summary>
    /// Crear planes de mantenimiento previsualizados
    /// </summary>
    /// <param name="user"> Usuario para verificar linea y permisos de generacion</param>
    /// <param name="planinfo"> Informacion acerca del plan</param>
    /// <param name="service"> Servicio de Planes</param>
    /// <returns> Tareas y Criterios del plan generado</returns>
    private static async Task<Created<PlanDto>> PreviewPlans(ClaimsPrincipal user, PlanPrevDto planinfo, IPlansService service)
    {
        PlanDto result = await service.CreatePlan(user, planinfo);
        return TypedResults.Created("", result);
    }


    /// <summary>
    /// Crear planes de mantenimiento que no cumplen los criterios establecidos
    /// </summary>
    /// <param name="user"> Usuario para verificar linea y permisos de generacion</param>
    /// <param name="planinfo"> Informacion acerca del plan</param>
    /// <param name="service"> Servicio de Planes</param>
    /// <returns> Tareas y Criterios del plan generado</returns>
    private static async Task<Created<PlanDto>> NonCompliancePlan(ClaimsPrincipal user, PlanPrevDto planinfo, IPlansService service)
    {
        PlanDto result = await service.NonCompliancePlan(user, planinfo);
        return TypedResults.Created("", result);
    }


    /// <summary>
    /// Filtrar planes de mantenimiento
    /// </summary>
    /// <param name="user"> Ver linea en caso de Tecnico</param>
    /// <param name="filter"> valores a filtrar</param>
    /// <param name="service"> Servicio de Planes</param>
    /// <returns> Planes filtrados</returns>
    private static async Task<Ok<List<PlanDto>>> FilterPlans(ClaimsPrincipal user, PlanFilterDto filter, IPlansService service)
    {
        List<PlanDto> result = await service.FilterPlans(user, filter);
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Obtener Plan de mantenimiento con un id determinado
    /// </summary>
    /// <param name="id"> id del plan de mantenimiento</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> Servicio de Planes</param>
    /// <returns></returns>
    private static async Task<Results<Ok<PlanViewDto>,NotFound>> ViewPlan(Guid id, ClaimsPrincipal user, IPlansService service)
    {
        var (result, notFound) = await service.ViewPlan(id, user);
        if(notFound)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Obtener Historial de un plan de mantenimiento
    /// </summary>
    /// <param name="id"> id del plan</param>
    /// <param name="service"> Servicio de Planes</param>
    /// <returns> Historial del plan o null en caso de no encontrar dicho plan</returns>
    private static async Task<Results<Ok<PlanTrazabilityDto>, NotFound>> TrazabilityPlan(Guid id, IPlansService service)
    {
        var (result, notFound) = await service.TrazabilityPlan(id);
        if(notFound)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(result);
    }

}