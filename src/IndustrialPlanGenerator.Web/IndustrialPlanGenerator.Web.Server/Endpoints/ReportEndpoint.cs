using Microsoft.AspNetCore.Http.HttpResults;
using IndustrialPlanGenerator.Web.Server.Auth;
using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.Reports;


public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints (this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var reportes = api.MapGroup("/reportes").RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.PlantManager, Policies.Administrator));

        // REP-1: GET /api/reportes/planes-generados?lineaId= [PDF]
        reportes.MapGet("/planes-generados", ReportGetGeneratedPlans)
        .WithName("GetReportofPlans")
        .WithSummary("Obtener datos del reporte sobre los planes y parametros de una linea")
        .Produces<List<ReportPlanParametersDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // REP-2: GET /api/reportes/tareas-mas-ejecutadas?lineaId=&top= [PDF]
        reportes.MapGet("/tareas-mas-ejecutadas", ReportMoreExecTask)
        .WithName("ReportMoreExecTask")
        .WithSummary("Obtener tareas mas ejecutadas de una linea")
        .Produces<List<ReportTasksDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // REP-3: GET /api/reportes/planes-validados?revisorId= [PDF]
        reportes.MapGet("/planes-validados", ReportValidPlans)
        .WithName("ValidatedPlans")
        .WithSummary("Obtener planes validados por un revisor")
        .Produces<List<ReportPlanValidationDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // REP-4: GET /api/reportes/desempeno-operarios?planId= [PDF]
        reportes.MapGet("/desempeno-operarios", ReportOpPerformance)
        .WithName("OperatorsPerformance")
        .WithSummary("Obtener rendimiento de los empleados")
        .Produces<List<ReportOperatorPerformanceDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // REP-5: GET /api/reportes/comparativa-planes?lineaIds=1,2,3 [PDF]
        reportes.MapGet("/comparativa-planes", GetComparationInLines)
        .WithName("ComparativeinLines")
        .WithSummary("Obtener Comparacion de planes en distintas lineas")
        .Produces<List<ReportComparativaPlanDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // REP-6: GET /api/reportes/correlacion-criticidad?lineaId= [PDF]
        reportes.MapGet("/correlacion-criticidad", GetCorrelation)
        .WithName("Correlation-Criticality")
        .WithSummary("Obtener Correlacion entre el rendimiento promedio de los empleados y la criticidad")
        .Produces<ReportLineAvgPerfDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // REP-7: GET /api/reportes/tareas-incumplidas?lineaId=&top=10 [PDF]
        reportes.MapGet("/tareas-incumplidas", GetNonComplianceTasks)
        .WithName("NonComplienceTasks")
        .WithSummary("Obtener Tareas de mayor tasa de incumplimiento")
        .Produces<List<ReportNonComplianceTasksDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
        
        // REP-8: GET /api/reportes/reinspeccion-vs-turno?lineaId= [PDF]
        reportes.MapGet("/reinspeccion-vs-turno", GetReinspectionVsTurn)
        .WithName("ReinspectionVsTurn")
        .WithSummary("Obtener relacion entre reinspecciones y el turno")
        .Produces<ReportReinspectionVsTurnDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }


    /// <summary>
    /// Obtener Reporte sobre los planes y parametros de una linea
    /// </summary>
    /// <param name="lineaId"> id de la linea</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Reportes</param>
    /// <returns> Reporte</returns>
    private static async Task<Results<Ok<List<ReportPlanParametersDto>>, NotFound, UnauthorizedHttpResult>> ReportGetGeneratedPlans(Guid lineaId, ClaimsPrincipal user, IReportService service)
    {
        var (report, lineidNotFound, techNoAuth) = await service.GetGeneratedPlans(lineaId, user);
        if(lineidNotFound)
        {
            return TypedResults.NotFound();
        }
        else if (techNoAuth)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(report);
    }


    /// <summary>
    /// Obtener tareas mas ejecutadas en una linea
    /// </summary>
    /// <param name="lineaId"> id de la linea</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Reportes</param>
    /// <returns> Tareas mas ejecutadas</returns>
    private static async Task<Results<Ok<List<ReportTasksDto>>, NotFound, UnauthorizedHttpResult>> ReportMoreExecTask(Guid lineaId, ClaimsPrincipal user, IReportService service)
    {
        var (report, lineidNotFound, techNoAuth) = await service.GetMoreExecTasks(lineaId, user);
        if(lineidNotFound)
        {
            return TypedResults.NotFound();
        }
        else if (techNoAuth)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(report);
    }


    /// <summary>
    /// Planes Validados por un revisor
    /// </summary>
    /// <param name="revisorId"> id del revisor</param>
    /// <param name="service"> servicio de Reportes</param>
    /// <returns> Planes validados, con sus validaciones (observaciones y fecha)</returns>
    private static async Task<Results<Ok<List<ReportPlanValidationDto>>, NotFound>> ReportValidPlans(Guid revisorId, IReportService service)
    {
        var (report, revieweridNotFound) = await service.GetValidPlans(revisorId);
        if(revieweridNotFound)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(report);
    }


    /// <summary>
    /// Obtener Desempeno de los operadores en un plan
    /// </summary>
    /// <param name="planId"> id del plan</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Reportes</param>
    /// <returns> Resultado del desempeno</returns>
    private static async Task<Results<Ok<List<ReportOperatorPerformanceDto>>, NotFound, UnauthorizedHttpResult>> ReportOpPerformance(Guid planId, ClaimsPrincipal user, IReportService service)
    {
        var (report, lineidNotFound, techNoAuth) = await service.GetPerformance(planId, user);
        if(lineidNotFound)
        {
            return TypedResults.NotFound();
        }
        else if (techNoAuth)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(report);
    }


    /// <summary>
    /// Realizar comparacion de los planes de distintas lineas
    /// </summary>
    /// <param name="lineaIds">identificadores de las lineas a comparar</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Reportes</param>
    /// <returns> Comparacion entre los planes que pertenecen a las lineas</returns>
    private static async Task<Results<Ok<List<ReportComparativaPlanDto>>, NotFound, UnauthorizedHttpResult>> GetComparationInLines(List<Guid> lineaIds, ClaimsPrincipal user, IReportService service)
    {
        var (report, lineidNotFound, techNoAuth) = await service.GetPlansinLines(lineaIds, user);
        if(lineidNotFound)
        {
            return TypedResults.NotFound();
        }
        else if (techNoAuth)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(report);
    }


    /// <summary>
    /// Obtener correlacion entre el desempeno promedio y la criticidad
    /// </summary>
    /// <param name="lineaId"> id de la linea</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Reportes</param>
    /// <returns> Reporte de correlacion entre el desempeno promedio y la criticidad</returns>
    private static async Task<Results<Ok<List<ReportLineAvgPerfDto>>, NotFound, UnauthorizedHttpResult>> GetCorrelation(Guid lineaId, ClaimsPrincipal user, IReportService service)
    {
        var (report, lineidNotFound, techNoAuth) = await service.GetCorrelation(lineaId, user);
        if(lineidNotFound)
        {
            return TypedResults.NotFound();
        }
        else if (techNoAuth)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(report);
    }


    /// <summary>
    /// Obtener reporte de tareas con mayor tasa de incumplimiento
    /// </summary>
    /// <param name="lineaId"> id de la linea</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Reportes</param>
    /// <returns> Tareas con mayor tasa de incumplimientp</returns>
    private static async Task<Results<Ok<List<ReportNonComplianceTasksDto>>, NotFound, UnauthorizedHttpResult>> GetNonComplianceTasks(Guid lineaId, ClaimsPrincipal user, IReportService service)
    {
        var (report, lineidNotFound, techNoAuth) = await service.GetNonComplianceTasks(lineaId, user);
        if(lineidNotFound)
        {
            return TypedResults.NotFound();
        }
        else if (techNoAuth)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(report);
    }


    /// <summary>
    /// Obtener reporte de reinspeccion contra operadores del turno
    /// </summary>
    /// <param name="lineaId"> id de la linea</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Reportes</param>
    /// <returns> Reporte de reinspeccion contra operadores del turno</returns>
    private static async Task<Results<Ok<ReportReinspectionVsTurnDto>, NotFound, UnauthorizedHttpResult>> GetReinspectionVsTurn(Guid lineaId, ClaimsPrincipal user, IReportService service)
    {
        var (report, lineidNotFound, techNoAuth) = await service.GetReinspVsTurn(lineaId, user);
        if(lineidNotFound)
        {
            return TypedResults.NotFound();
        }
        else if (techNoAuth)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(report);
    }


}