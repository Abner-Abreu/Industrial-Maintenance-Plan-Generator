using IndustrialPlanGenerator.Domain.Telemetry;
using IndustrialPlanGenerator.Domain.Enums;
namespace IndustrialPlanGenerator.Services.Contracts.Telemetry;


// Telemetry-1

/// <summary>
/// Insertar Lecturas a un sensor
/// </summary>
/// <param name="Reading"> Lecturas a ingresar de un Sensor</param>
public sealed record TelemetryReadingDto(SensorReading Reading);


/// <summary>
/// Informacion Procesada del ingreso de Lecturas
/// </summary>
/// <param name="Inserted"> Lecturas insertadas</param>
/// <param name="Duplicates"> Lecturas Duplicadas</param>
/// <param name="Rejected"> Lecturas Rechazadas por sensor desconocido</param>
public sealed record TelemetryCallBackDto(int Inserted, int Duplicates, int Rejected);





// Telemetry-2

public sealed record TelemetryGranularity(bool hour, bool day, bool week, bool month);

/// <summary>
/// Obtener Informacion adicional de los sensores
/// </summary>
/// <param name="id"> id del sensor</param>
/// <param name="Name"> nombre del sensor</param>
/// <param name="lineId"> id de la linea</param>
/// <param name="Start"> Tiempo de inicio del muestreo</param>
/// <param name="Finish"> Tiempo final del muestreo</param>
/// <param name="Granularity"></param>
/// <param name="type"> tipo de sensor (opcional)</param>
public sealed record TelemetryAggregateParamsDto(Guid id, string Name, Guid lineId, DateTime Start, DateTime Finish, TelemetryGranularity Granularity, SensorType? type);


/// <summary>
/// Resultados del analisis de los agregados
/// </summary>
/// <param name="Min"> Valor Minimo</param>
/// <param name="Max"> Valor Maximo</param>
/// <param name="Avg"> Valor Promedio</param>
/// <param name="QuantityPeriod"> Cantidad por Periodo</param>
public sealed record TelemetryAggregatesInfoDto(double Min, double Max, double Avg, int QuantityPeriod);