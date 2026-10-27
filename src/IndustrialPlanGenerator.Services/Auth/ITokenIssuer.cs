using IndustrialPlanGenerator.Services.Contracts.Auth;

namespace IndustrialPlanGenerator.Services.Auth;

/// <summary>Token emitido y su momento de expiración.</summary>
/// <param name="Token">Token firmado (JWT).</param>
/// <param name="ExpiresAt">Momento (UTC) en que deja de ser válido.</param>
public sealed record IssuedToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>
/// Puerto para emitir el token de sesión. La implementación (JWT) está en Web.Server, donde también
/// se leen la clave de firma y la duración desde la configuración.
/// </summary>
public interface ITokenIssuer
{
    /// <summary>Emite un token que contiene el id, el nombre y el rol del usuario.</summary>
    /// <param name="user">Usuario autenticado.</param>
    IssuedToken Issue(SessionUserDto user);
}
