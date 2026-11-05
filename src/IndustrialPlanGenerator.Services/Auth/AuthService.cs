using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Services.Contracts.Auth;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Services.Exceptions;
using IndustrialPlanGenerator.Domain.Exceptions;
namespace IndustrialPlanGenerator.Services.Auth;

/// <inheritdoc cref="IAuthService"/>
public sealed class AuthService(
    IUnitOfWork uow,
    IPasswordHasher hasher,
    ITokenIssuer tokens) : IAuthService
{
    /// <summary>Longitud mínima de una contraseña nueva (decisión pendiente de confirmar con el equipo).</summary>
    public const int MinimumPasswordLength = 8;
    public async Task<LoginResultDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrEmpty(request.Password))
            throw new AuthenticationFailedException();

        var user = await uow.Users.GetByUserNameAsync(request.UserName.Trim(), ct);

        // Se verifica siempre, aunque el usuario no exista (hash nulo), para que el tiempo de
        // respuesta no permita averiguar qué usuarios están registrados.
        var passwordIsValid = hasher.Verify(user?.Credentials, request.Password);

        if (user is null || !passwordIsValid)
            throw new AuthenticationFailedException();

        var session = new SessionUserDto(user.Id, user.Name, user.Role.ToString());
        var token = tokens.Issue(session);

        return new LoginResultDto(token.Token, token.ExpiresAt, session);
    }


    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByIdAsync(userId, ct)
            ?? throw new AuthenticationFailedException();

        // Primero se confirma la contraseña actual; así una sesión robada no sirve para cambiarla sin conocerla.
        if (string.IsNullOrEmpty(request.CurrentPassword) || !hasher.Verify(user.Credentials, request.CurrentPassword))
            throw new DomainException("La contraseña actual es incorrecta.");

        if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < MinimumPasswordLength)
            throw new DomainException($"La contraseña nueva debe tener al menos {MinimumPasswordLength} caracteres.");

        if (request.NewPassword == request.CurrentPassword)
            throw new DomainException("La contraseña nueva debe ser distinta de la actual.");

        user.Credentials = hasher.Hash(request.NewPassword);
        uow.Users.Update(user);
        await uow.SaveChangesAsync(ct);
    }
}
