using System.Security.Claims;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Auth;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Endpoints de autenticación.</summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Inicia sesión y devuelve el token JWT con el rol del usuario.")
            .Produces<LoginResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", Me)
            .RequireAuthorization()
            .WithName("CurrentUser")
            .WithSummary("Devuelve los datos del usuario de la sesión actual.")
            .Produces<SessionUserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }

    // Credenciales incorrectas: AuthService lanza AuthenticationFailedException y
    // GlobalExceptionHandler la convierte en 401 ProblemDetails.
    private static async Task<Ok<LoginResultDto>> Login(
        [FromBody] LoginRequest request, IAuthService auth, CancellationToken ct)
        => TypedResults.Ok(await auth.LoginAsync(request, ct));
    private static Results<Ok<SessionUserDto>, UnauthorizedHttpResult> Me(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtTokenIssuer.SubjectClaim);
        var name = principal.FindFirstValue(JwtTokenIssuer.NameClaim);
        var role = principal.FindFirstValue(JwtTokenIssuer.RoleClaim);

        if (!Guid.TryParse(sub, out var id) || name is null || role is null)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(new SessionUserDto(id, name, role));
    }
}