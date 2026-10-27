namespace IndustrialPlanGenerator.Web.Server.Auth;

/// <summary>
/// Configuración del token JWT.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Longitud mínima de la clave de firma (HMAC-SHA256).</summary>
    public const int MinimumKeyLength = 32;

    /// <summary>Quién emite el token.</summary>
    public string Issuer { get; set; } = "IndustrialPlanGenerator";

    /// <summary>Para quién se emite el token.</summary>
    public string Audience { get; set; } = "IndustrialPlanGenerator.Web";

    /// <summary>Clave secreta de firma (mínimo 32 caracteres). Viene de user-secrets o del entorno.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Vida del token en minutos.</summary>
    public int ExpirationMinutes { get; set; } = 60;
}