using IndustrialPlanGenerator.Services.Contracts.Auth;
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


     /// <summary>
    /// Cambia la contraseña del usuario autenticado.
    /// </summary>
    /// <param name="userId">Id del usuario de la sesión (sale del token, nunca del cliente).</param>
    /// <param name="request">Contraseña actual y contraseña nueva.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="Exceptions.AuthenticationFailedException">El usuario del token ya no existe (HTTP 401).</exception>
    /// <exception cref="Domain.Exceptions.DomainException">
    /// La contraseña actual es incorrecta, la nueva es demasiado corta o es igual a la actual (HTTP 422).
    /// </exception>
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}