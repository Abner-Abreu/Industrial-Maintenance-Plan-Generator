namespace IndustrialPlanGenerator.Services.Contracts.Auth;

/// <summary>Datos que envía el cliente para iniciar sesión.</summary>
/// <param name="UserName">Nombre de usuario (corresponde a <c>User.Name</c>).</param>
/// <param name="Password">Contraseña en texto plano; solo viaja por HTTPS y nunca se guarda.</param>
public sealed record LoginRequest(string UserName, string Password);
