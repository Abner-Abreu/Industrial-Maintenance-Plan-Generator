using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Services.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Errors;

/// <summary>
/// Convierte las excepciones lanzadas por los servicios en respuestas <c>ProblemDetails</c> con el
/// código HTTP correcto. Solo actúa sobre rutas <c>/api</c>; las páginas Blazor siguen con <c>/Error</c>.
/// Para añadir un nuevo tipo de excepción de negocio, agregue una línea al <c>switch</c>.
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
