using IndustrialPlanGenerator.Services.Contracts.Sensors;
using IndustrialPlanGenerator.Services.Sensors;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Endpoints de sensores (SEN-1 y SEN-2).</summary>
public static class SensorEndpoints
{
    public static IEndpointRouteBuilder MapSensorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/systems/{systemId:guid}/sensors").WithTags("Sensors");

        // Administrador y jefe de planta: cualquier sistema. Técnico: solo si tiene acceso a la línea
        // (lo comprueba el servicio). Operario: sin acceso (403 por la política).
        group.MapGet("/", List)
            .RequireAuthorization(p => p.RequireRole(Policies.Administrator, Policies.PlantManager, Policies.Technician))
            .WithName("ListSystemSensors")
            .WithSummary("Sensores de un sistema (SEN-1).")
            .WithDescription("Administrador y jefe de planta ven cualquier sistema; el técnico, solo los de sus líneas.")
            .Produces<IReadOnlyList<SensorDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .RequireAuthorization(Policies.Administrator)
            .WithName("CreateSensor")
            .WithSummary("Da de alta un sensor en un sistema (SEN-2).")
            .WithDescription("Tipos: temperatura, vibración o presión. Los sensores no se eliminan: las lecturas históricas dependen de ellos.")
            .Produces<SensorDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<SensorDto>>> List(
        Guid systemId, ISensorService sensors, CancellationToken ct)
        => TypedResults.Ok(await sensors.ListBySystemAsync(systemId, ct));

    private static async Task<Created<SensorDto>> Create(
        Guid systemId, [FromBody] CreateSensorRequest request, ISensorService sensors, CancellationToken ct)
    {
        var created = await sensors.CreateAsync(systemId, request, ct);
        return TypedResults.Created($"/api/systems/{systemId}/sensors/{created.Id}", created);
    }
}
