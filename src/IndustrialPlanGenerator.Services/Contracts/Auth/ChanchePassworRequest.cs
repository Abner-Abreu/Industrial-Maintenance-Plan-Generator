namespace IndustrialPlanGenerator.Services.Contracts.Auth;

/// <summary>Datos para cambiar la contraseña del usuario autenticado (AUT-3).</summary>
/// <param name="CurrentPassword">Contraseña actual, para confirmar que es el propio usuario.</param>
/// <param name="NewPassword">Contraseña nueva (mínimo 8 caracteres y distinta de la actual).</param>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);