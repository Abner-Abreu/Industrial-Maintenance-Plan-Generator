using System.Security.Claims;
namespace IndustrialPlanGenerator.Services.Contracts.Telemetry;

public interface ITelemetryService
{
    /// <summary>
    /// Insertar lecturas de telemetria
    /// </summary>
    /// <param name="reading"> lecturas</param>
    /// <returns> resultados de la inserccion</returns>
    Task<TelemetryCallBackDto> InsertTelemetryReadings(TelemetryReadingDto reading);


    /// <summary>
    /// Obtener agregados de los sensores
    /// </summary>
    /// <param name="parameters"> parametros de los agregados</param>
    /// <param name="user"> usuario (Tecnico solo puede ver sensores de su linea)</param>
    /// <returns> informacion de los agregados, y booleano de autorizacion</returns>
    Task<(TelemetryAggregatesInfoDto Information, bool Unauthorized)> GetTelemetryAggregates(TelemetryAggregateParamsDto parameters, ClaimsPrincipal user);

}