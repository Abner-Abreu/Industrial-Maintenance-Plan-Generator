using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Contracts.Auth;
using IndustrialPlanGenerator.Services.Exceptions;

namespace IndustrialPlanGenerator.Services.Auth;

/// <inheritdoc cref="IAuthService"/>
public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher hasher,
    ITokenIssuer tokens) : IAuthService
{
    public async Task<LoginResultDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrEmpty(request.Password))
            throw new AuthenticationFailedException();

        var user = await users.GetByUserNameAsync(request.UserName.Trim(), ct);

        // Se verifica siempre, aunque el usuario no exista (hash nulo), para que el tiempo de
        // respuesta no permita averiguar qué usuarios están registrados.
        var passwordIsValid = hasher.Verify(user?.Credentials, request.Password);

        if (user is null || !passwordIsValid)
            throw new AuthenticationFailedException();

        var session = new SessionUserDto(user.Id, user.Name, user.Role.ToString());
        var token = tokens.Issue(session);

        return new LoginResultDto(token.Token, token.ExpiresAt, session);
    }
}
