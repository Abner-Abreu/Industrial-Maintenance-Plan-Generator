using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Exceptions;

namespace IndustrialPlanGenerator.Web.Server.Auth;

/// <summary>Gets the session user from the token claims (sub and role).</summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirst(JwtTokenIssuer.SubjectClaim)?.Value, out var id)
            ? id
            : throw new AuthenticationFailedException();

    public UserRole Role =>
        Enum.TryParse<UserRole>(accessor.HttpContext?.User.FindFirst(JwtTokenIssuer.RoleClaim)?.Value, out var role)
            ? role
            : throw new AuthenticationFailedException();
}
