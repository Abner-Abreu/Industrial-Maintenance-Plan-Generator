using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Services.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Errors;

/// <summary>
/// Converts the exceptions thrown by the services into <c>ProblemDetails</c> responses with the
/// correct HTTP status code. It only acts on <c>/api</c> routes; Blazor pages continue with <c>/Error</c>.
/// To add a new type of business exception, add a line to the <c>switch</c>.
/// </summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api"))
            return false;

        var (status, title, detail) = exception switch
        {
            AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "Credenciales inválidas", exception.Message),
            InvalidRequestException => (StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Acceso denegado", exception.Message),
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto", exception.Message),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio violada", exception.Message),
            NotImplementedException => (StatusCodes.Status501NotImplemented, "Funcionalidad aún no implementada", "Este endpoint todavía no está implementado."),
            // Error inesperado: no se filtra el mensaje interno al cliente.
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor", "Ocurrió un error inesperado."),
        };

        httpContext.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail },
        });
    }
}
