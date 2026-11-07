namespace IndustrialPlanGenerator.Services.Exceptions;

/// <summary>Conflict with the current state (for example, an existing username). HTTP 409.</summary>
public sealed class ConflictException(string message) : Exception(message);
