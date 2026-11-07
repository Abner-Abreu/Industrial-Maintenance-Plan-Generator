namespace IndustrialPlanGenerator.Services.Exceptions;

/// <summary>The input data is invalid (empty field, unknown value, disallowed combination). HTTP 400.</summary>
public sealed class InvalidRequestException(string message) : Exception(message);
