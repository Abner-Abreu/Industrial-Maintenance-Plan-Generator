using System.Text;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IndustrialPlanGenerator.Web.Server.Auth;

/// <summary>
/// Emite tokens JWT firmados con HMAC-SHA256. Claims: <c>sub</c> (id), <c>name</c> y <c>role</c>.
/// Usa <see cref="TimeProvider"/> para poder fijar la hora en las pruebas.
/// </summary>
public sealed class JwtTokenIssuer(IOptions<JwtOptions> options, TimeProvider clock) : ITokenIssuer
{
    public const string SubjectClaim = "sub";
    public const string NameClaim = "name";
    public const string RoleClaim = "role";

    private readonly JwtOptions _options = options.Value;

    public IssuedToken Issue(SessionUserDto user)
    {
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.ExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                [SubjectClaim] = user.Id.ToString(),
                [NameClaim] = user.Name,
                [RoleClaim] = user.Role
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new IssuedToken(token, expiresAt);
    }
}
