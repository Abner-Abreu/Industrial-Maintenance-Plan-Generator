namespace IndustrialPlanGenerator.Services.Contracts.Sensors;

/// <summary>Sensor de un equipo (SEN-1, SEN-2).</summary>
/// <param name="Id">Id del sensor.</param>
/// <param name="SystemId">Id del sistema (equipo) al que pertenece.</param>
/// <param name="Type">Tipo de sensor: nombre del valor de <c>SensorType</c> (temperatura, vibración o presión).</param>
public sealed record SensorDto(Guid Id, Guid SystemId, string Type);

/// <summary>Datos para dar de alta un sensor (SEN-2).</summary>
/// <param name="Type">Tipo de sensor (obligatorio); debe ser un valor válido de <c>SensorType</c>.</param>
public sealed record CreateSensorRequest(string Type);
