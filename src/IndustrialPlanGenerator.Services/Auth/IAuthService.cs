namespace IndustrialPlanGenerator.Services.Contracts.Auth;

public interface IAuthService
{
    /// <summary>
    /// Valida las credenciales y emite el token de sesión.
    /// </summary>
    /// <param name="request">Nombre de usuario y contraseña.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El token, su expiración y los datos del usuario (incluido su rol).</returns>
    /// <exception cref="Exceptions.AuthenticationFailedException">
    /// Datos vacíos, usuario inexistente o contraseña incorrecta. En los tres casos se lanza la misma
    /// excepción con el mismo mensaje, para no revelar cuál fue el motivo (HTTP 401).
    /// </exception>
    Task<LoginResultDto> LoginAsync(LoginRequest request, CancellationToken ct = default);
}