namespace IndustrialPlanGenerator.Services.Auth;

/// <summary>
/// Puerto para el hash de contraseñas. La implementación está en Web.Server; los servicios solo
/// conocen esta interfaz, de modo que el algoritmo se puede cambiar sin tocarlos.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Genera el hash (con sal aleatoria) que se guarda en <c>User.Credentials</c>.</summary>
    /// <param name="password">Contraseña en texto plano.</param>
    string Hash(string password);

    /// <summary>
    /// Comprueba una contraseña contra un hash guardado.
    /// </summary>
    /// <param name="passwordHash">
    /// Hash guardado. Si es <c>null</c> (usuario inexistente) o tiene un formato inválido, devuelve
    /// <c>false</c> tras hacer el mismo trabajo de cálculo, para que el tiempo de respuesta no revele
    /// si el usuario existe.
    /// </param>
    /// <param name="password">Contraseña en texto plano.</param>
    bool Verify(string? passwordHash, string password);
}
