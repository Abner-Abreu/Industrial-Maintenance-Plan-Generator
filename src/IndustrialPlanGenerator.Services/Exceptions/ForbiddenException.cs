namespace IndustrialPlanGenerator.Services.Exceptions;

/// <summary>The authenticated user does not have permission for this operation. HTTP 403.</summary>
public sealed class ForbiddenException() : Exception("No tiene permiso para realizar esta operación.");
