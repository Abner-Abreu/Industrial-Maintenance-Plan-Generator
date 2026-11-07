using IndustrialPlanGenerator.Domain.Enums;

namespace IndustrialPlanGenerator.Services.Auth;

/// <summary>
/// Current session user. Services use it instead of <c>HttpContext</c>, so they do not depend on the
/// web layer and can be tested. The implementation is in Web.Server.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Authenticated user ID.</summary>
    Guid Id { get; }

    /// <summary>Authenticated user role.</summary>
    UserRole Role { get; }
}
