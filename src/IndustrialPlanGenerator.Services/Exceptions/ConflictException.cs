namespace IndustrialPlanGenerator.Services.Exceptions;

/// <summary>Conflicto con el estado actual (por ejemplo, un nombre de usuario ya existente). HTTP 409.</summary>
public sealed class ConflictException(string message) : Exception(message);
