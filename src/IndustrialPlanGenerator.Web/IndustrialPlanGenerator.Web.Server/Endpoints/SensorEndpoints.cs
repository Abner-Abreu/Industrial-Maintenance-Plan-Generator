using IndustrialPlanGenerator.Services.Contracts.Sensors;
using IndustrialPlanGenerator.Services.Sensors;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Sensor endpoints (SEN-1 and SEN-2).</summary>
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
            .WithSummary("Sensors of a system (SEN-1).")
            .WithDescription("Administrator and plant manager see any system; technicians, only those of their lines.")
            .Produces<IReadOnlyList<SensorDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .RequireAuthorization(Policies.Administrator)
            .WithName("CreateSensor")
            .WithSummary("Registers a sensor in a system (SEN-2).")
            .WithDescription("Types: temperature, vibration, or pressure. Sensors are not deleted: historical readings depend on them.")
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
