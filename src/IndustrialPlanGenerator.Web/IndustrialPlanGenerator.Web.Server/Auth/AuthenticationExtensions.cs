using System.Text;
using IndustrialPlanGenerator.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IndustrialPlanGenerator.Web.Server.Auth;

/// <summary>Nombres de las políticas de autorización (una por rol; coinciden con <c>UserRole</c>).</summary>
public static class Policies
{
    public const string Administrator = nameof(Administrator);
    public const string Technician = nameof(Technician);
    public const string Operator = nameof(Operator);
    public const string PlantManager = nameof(PlantManager);
}

public static class AuthenticationExtensions
{
    /// <summary>
    /// Registra el hasher, el emisor de tokens, la autenticación JWT y las políticas por rol.
    /// Falla al arrancar si falta la clave de firma o es demasiado corta.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => o.Key is { Length: >= JwtOptions.MinimumKeyLength },
                $"Jwt:Key debe tener al menos {JwtOptions.MinimumKeyLength} caracteres " +
                "(configúrala con user-secrets o variable de entorno Jwt__Key).")
            .Validate(o => o.ExpirationMinutes > 0, "Jwt:ExpirationMinutes debe ser mayor que cero.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();

        // La validación del token lee las mismas opciones que usa el emisor.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                var o = jwt.Value;
                bearer.MapInboundClaims = false; // se conservan los nombres cortos: sub, name, role
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = o.Issuer,
                    ValidateAudience = true,
                    ValidAudience = o.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtTokenIssuer.NameClaim,
                    RoleClaimType = JwtTokenIssuer.RoleClaim
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Administrator, p => p.RequireRole(Policies.Administrator))
            .AddPolicy(Policies.Technician, p => p.RequireRole(Policies.Technician))
            .AddPolicy(Policies.Operator, p => p.RequireRole(Policies.Operator))
            .AddPolicy(Policies.PlantManager, p => p.RequireRole(Policies.PlantManager));

        return services;
    }
}
