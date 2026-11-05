namespace IndustrialPlanGenerator.Services.Exceptions;

/// <summary>El usuario autenticado no tiene permiso para esta operación. HTTP 403.</summary>
public sealed class ForbiddenException() : Exception("No tiene permiso para realizar esta operación.");
