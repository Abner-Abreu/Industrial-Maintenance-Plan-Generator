namespace IndustrialPlanGenerator.Services.Exceptions;

/// <summary>Los datos de entrada son inválidos (campo vacío, valor desconocido, combinación no permitida). HTTP 400.</summary>
public sealed class InvalidRequestException(string message) : Exception(message);
