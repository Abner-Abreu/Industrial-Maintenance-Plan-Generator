using System.Security.Cryptography;
using IndustrialPlanGenerator.Services.Auth;

namespace IndustrialPlanGenerator.Web.Server.Auth;

/// <summary>
/// Hash de contraseñas con PBKDF2-HMAC-SHA256.
/// Formato guardado: <c>v1.{iteraciones}.{sal en base64}.{hash en base64}</c>. Guarda las iteraciones
/// dentro del propio hash, así se pueden subir en el futuro sin invalidar las contraseñas ya guardadas.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    
    public const int DefaultIterations = 600_000;

    private const string FormatVersion = "v1";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int MaxIterations = 10_000_000;   // tope defensivo al leer un hash guardado
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    private static readonly byte[] DummySalt = new byte[SaltSize];

    private readonly int _iterations;

    /// <param name="iterations">Iteraciones para los hashes nuevos.</param>
    public Pbkdf2PasswordHasher(int iterations = DefaultIterations)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
        _iterations = iterations;
    }

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, _iterations, Algorithm, KeySize);

        return $"{FormatVersion}.{_iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string? passwordHash, string password)
    {
        if (!TryParse(passwordHash, out var iterations, out var salt, out var expectedKey))
        {
            // Hash ausente o inválido: se hace el mismo cálculo para no delatar la diferencia por tiempo.
            Rfc2898DeriveBytes.Pbkdf2(password, DummySalt, _iterations, Algorithm, KeySize);
            return false;
        }

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }

    private static bool TryParse(string? hash, out int iterations, out byte[] salt, out byte[] key)
    {
        iterations = 0;
        salt = [];
        key = [];

        if (string.IsNullOrEmpty(hash))
            return false;

        var parts = hash.Split('.');
        if (parts.Length != 4 || parts[0] != FormatVersion)
            return false;

        if (!int.TryParse(parts[1], out iterations) || iterations < 1 || iterations > MaxIterations)
            return false;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            key = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        return salt.Length > 0 && key.Length > 0;
    }
}
