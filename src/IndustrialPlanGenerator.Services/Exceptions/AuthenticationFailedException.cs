namespace IndustrialPlanGenerator.Services.Exceptions;

/// <summary>
/// Credenciales inválidas. Se lanza igual si el usuario no existe o si la contraseña es incorrecta,
/// con el mismo mensaje, para no revelar cuál de las dos falló. La API la traduce a HTTP 401.
/// </summary>
public sealed class AuthenticationFailedException()
    : Exception("Usuario o contraseña incorrectos.");