using IndustrialPlanGenerator.Domain.Enums;

namespace IndustrialPlanGenerator.Services.Auth;

/// <summary>
/// Usuario de la sesión actual. Los servicios lo usan en lugar de <c>HttpContext</c>, para que no
/// dependan de la capa web y se puedan probar. La implementación está en Web.Server.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Id del usuario autenticado.</summary>
    Guid Id { get; }

    /// <summary>Rol del usuario autenticado.</summary>
    UserRole Role { get; }
}
