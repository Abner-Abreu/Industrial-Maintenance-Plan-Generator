using Microsoft.AspNetCore.Http.HttpResults;
using IndustrialPlanGenerator.Web.Server.Auth;
using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.TaskBank;
using IndustrialPlanGenerator.Services.Contracts.Telemetry;


public static class TelemetryEndpoints
{
    public static IEndpointRouteBuilder MapTelemetryEndpoints (this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var telemetria = api.MapGroup("/telemetria");

        // TEL-1: POST /api/telemetria/lecturas
        telemetria.MapPost("/lecturas", InsertSensorReadings)
        .WithName("InsertTelemetryReadings")
        .WithSummary("Insertar Lecturas de un sensor.")
        .Produces<TelemetryCallBackDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();  // Maneja su propia autenticación por cabecera de API Key
        
        // TEL-2: GET /api/telemetria/agregado [PDF]
        telemetria.MapGet("/agregado", GetTelemetryAggregates)
        .WithName("GetTelemetryAggregates")
        .WithSummary("Obtener agregados de los sensores.")
        .Produces<TelemetryAggregatesInfoDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.Administrator, Policies.PlantManager));

        return app;
    }


    /// <summary>
    /// Insertar lecturas de los sensores a la DB
    /// </summary>
    /// <param name="readings"> lecturas obtenidas</param>
    /// <param name="service"> servicio de Telemetria</param>
    /// <param name="context"> contexto http para la autentificacion (clave de API de la Planta)</param>
    /// <returns> Resultados de la inserccion</returns>
    private static async Task<Results<Ok<TelemetryCallBackDto>, UnauthorizedHttpResult>> InsertSensorReadings(TelemetryReadingDto readings, ITelemetryService service, HttpContext context)
    {
        // Autenticación especial
        if (!context.Request.Headers.TryGetValue("X-Plant-ApiKey", out var apiKey))
        {
            return TypedResults.Unauthorized();
        }
        
        TelemetryCallBackDto result = await service.InsertTelemetryReadings(readings);

        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Obtener agregados de los sensores
    /// </summary>
    /// <param name="parameters"> parametros de los agregados</param>
    /// <param name="user"> usuario</param>
    /// <param name="service"> servicio de Telemetria</param>
    /// <returns></returns>
    private static async Task<Results<Ok<TelemetryAggregatesInfoDto>, UnauthorizedHttpResult>> GetTelemetryAggregates(TelemetryAggregateParamsDto parameters, ClaimsPrincipal user, ITelemetryService service)
    {
        var (Informacion, Unauthorized) = await service.GetTelemetryAggregates(parameters, user);
        
        if(Unauthorized)
        {
            return TypedResults.Unauthorized();
        }
        return TypedResults.Ok(Informacion);
    }

}