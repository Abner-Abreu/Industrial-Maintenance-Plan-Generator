namespace IndustrialPlanGenerator.Services.Contracts.Sensors;

/// <summary>Sensor of an equipment (SEN-1, SEN-2).</summary>
/// <param name="Id">Sensor ID.</param>
/// <param name="SystemId">ID of the system (equipment) it belongs to.</param>
/// <param name="Type">Sensor type: name of the <c>SensorType</c> value (temperature, vibration, or pressure).</param>
public sealed record SensorDto(Guid Id, Guid SystemId, string Type);

/// <summary>Data for registering a sensor (SEN-2).</summary>
/// <param name="Type">Sensor type (required); must be a valid <c>SensorType</c> value.</param>
public sealed record CreateSensorRequest(string Type);
