using Microsoft.AspNetCore.Http.HttpResults;
using IndustrialPlanGenerator.Web.Server.Auth;
using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.Plans;
using IndustrialPlanGenerator.Services.Contracts.PlanExec;


public static class PlanExecEndpoints
{
    public static IEndpointRouteBuilder MapPlanExecEndpoints (this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var asignaciones = api.MapGroup("/asignaciones");



        // EJE-1: GET /api/mis-asignaciones?lineaId=
        api.MapGet("/mis-asignaciones", MyAssigs)
        .WithName("MyAssigs")
        .WithSummary("Ver asignaciones con su respectivo avance")
        .Produces<List<ExecProgressDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Operator));

        // EJE-2: GET /api/asignaciones/{id}
        asignaciones.MapGet("/{id:guid}", MyAssigbyId)
        .WithName("MyAssigbyId")
        .WithSummary("Ver una asignacion especificada por id")
        .Produces<ExecProgressbyAssig>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Operator, Policies.Technician, Policies.Administrator, Policies.PlantManager));
        
        // EJE-3: POST /api/asignaciones/{id}/ejecuciones
        asignaciones.MapPost("/{id:Guid}/ejecuciones", CreateExec)
        .WithName("CreateExec")
        .WithSummary("Ingresar una ejecucion del plan de mantenimiento")
        .Produces<PlanExecutionsDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Operator));
        
        
        return app;
    }

    /// <summary>
    /// Obtener Tareas y pogresos de la assignacion
    /// </summary>
    /// <param name="lineaid"> id de la linea</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Ejecucion de Planes</param>
    /// <returns> Planes de Mantenimiento asignados con los progresos de estas</returns>
    public static async Task<Results<Ok<List<ExecProgressDto>>, NotFound>> MyAssigs(Guid lineaid, ClaimsPrincipal user, IPlanExecService service)
    {
        var (result, notFound) = await service.GetExecutionProgress(lineaid, user);
        if (notFound)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Obtener Tareas y pogresos de la assignacion
    /// </summary>
    /// <param name="id"> id de la asignacion</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Ejecucion de Planes</param>
    /// <returns> Planes de Mantenimiento asignados con los progresos de estas o null en caso de no existir dicha asignacion</returns>
    public static async Task<Results<Ok<ExecProgressbyAssig>,NotFound>> MyAssigbyId(Guid id, ClaimsPrincipal user, IPlanExecService service)
    {
        var (result, notFound) = await service.GetExecutionbyId(id, user);
        if (notFound)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Registra la ejecucion de una tarea de una asignacion
    /// </summary>
    /// <param name="id"> id de la tarea</param>
    /// <param name="execinfo"> informacion de la ejecucion</param>
    /// <param name="service"> Servicio de Ejecucion de Planes</param>
    /// <returns> Respuestas de peticiones created, unprocessable y conflict sin valor de retorno</returns>
    public static async Task<Results<Created<PlanExecutionsDto>, UnprocessableEntity, Conflict>> CreateExec(Guid id, ExecCreateDto execinfo, IPlanExecService service)
    {
        var (result, NotAssig, NoTaskPlan, IsRegistered) = await service.CreateExecution(id, execinfo);

        if (NotAssig || NoTaskPlan)
        {
            return TypedResults.UnprocessableEntity();
        }
        else if (IsRegistered)
        {
            return TypedResults.Conflict();
        }
        return TypedResults.Created("", result);
    }

}

