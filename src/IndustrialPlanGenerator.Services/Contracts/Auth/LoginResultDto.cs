namespace IndustrialPlanGenerator.Services.Contracts.Auth;

/// <summary>Respuesta de un inicio de sesión correcto.</summary>
/// <param name="Token">Token JWT que el cliente envía en la cabecera <c>Authorization: Bearer</c>.</param>
/// <param name="ExpiresAt">Momento (UTC) en que el token deja de ser válido.</param>
/// <param name="User">Datos del usuario autenticado.</param>
public sealed record LoginResultDto(string Token, DateTimeOffset ExpiresAt, SessionUserDto User);
