using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Web.Client.Services;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace IndustrialPlanGenerator.Tests.Web.Client.Services;

/// <summary>
/// Verifies that <see cref="JwtAuthStateProvider"/> maps the JWT "name" and "role" claims
/// to the identity's native Name and role checks (Identity.Name, IsInRole).
/// The backend (JwtTokenIssuer) emits the claims: sub, name, role.
/// </summary>
public class JwtAuthStateProviderClaimsTests
{
    private const string SigningKey = "test-signing-key-with-at-least-32-characters!!";

    // ----- helpers (private to this class to avoid clashing with other test files) -----

    private static string CreateToken(DateTime expiresUtc, params Claim[] claims)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: expiresUtc.AddHours(-2),
            expires: expiresUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static JwtAuthStateProvider CreateProvider(string? storedToken)
    {
        var storage = new Mock<ILocalStorageService>();
        storage
            .Setup(s => s.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        return new JwtAuthStateProvider(storage.Object);
    }

    // ----- tests -----

    [Fact]
    public async Task ValidToken_ExposesNameThroughIdentityName()
    {
        var token = CreateToken(
            DateTime.UtcNow.AddHours(1),
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", "tuto"),
            new Claim("role", "Admin"));
        var provider = CreateProvider(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.Equal("tuto", state.User.Identity?.Name);
    }

    [Fact]
    public async Task ValidToken_MapsRoleClaimToIsInRole()
    {
        var token = CreateToken(
            DateTime.UtcNow.AddHours(1),
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", "tuto"),
            new Claim("role", "Admin"));
        var provider = CreateProvider(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.IsInRole("Admin"));
        Assert.False(state.User.IsInRole("SomeOtherRole"));
    }

    [Fact]
    public async Task ValidToken_WithMultipleRoles_UserIsInAllOfThem()
    {
        var token = CreateToken(
            DateTime.UtcNow.AddHours(1),
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", "tuto"),
            new Claim("role", "Admin"),
            new Claim("role", "Planner"));
        var provider = CreateProvider(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.IsInRole("Admin"));
        Assert.True(state.User.IsInRole("Planner"));
    }

    [Fact]
    public async Task ValidToken_KeepsSubjectClaimUntouched()
    {
        var userId = Guid.NewGuid().ToString();
        var token = CreateToken(
            DateTime.UtcNow.AddHours(1),
            new Claim("sub", userId),
            new Claim("name", "tuto"),
            new Claim("role", "Admin"));
        var provider = CreateProvider(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.Equal(userId, state.User.FindFirst("sub")?.Value);
    }

    [Fact]
    public async Task ValidToken_IsAuthenticatedWithJwtAuthenticationType()
    {
        var token = CreateToken(
            DateTime.UtcNow.AddHours(1),
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", "tuto"),
            new Claim("role", "Admin"));
        var provider = CreateProvider(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.Identity?.IsAuthenticated);
        Assert.Equal("jwt", state.User.Identity?.AuthenticationType);
    }

    [Fact]
    public async Task ExpiredToken_ReturnsAnonymousUserWithoutNameOrRoles()
    {
        var token = CreateToken(
            DateTime.UtcNow.AddHours(-1),
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", "tuto"),
            new Claim("role", "Admin"));
        var provider = CreateProvider(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity?.IsAuthenticated);
        Assert.Null(state.User.Identity?.Name);
        Assert.False(state.User.IsInRole("Admin"));
    }

    [Fact]
    public async Task MissingToken_ReturnsAnonymousUser()
    {
        var provider = CreateProvider(null);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity?.IsAuthenticated);
        Assert.Null(state.User.Identity?.Name);
    }

    [Fact]
    public async Task MalformedToken_ReturnsAnonymousUserAndDoesNotThrow()
    {
        var provider = CreateProvider("this-is-not-a-jwt");

        var exception = await Record.ExceptionAsync(() => provider.GetAuthenticationStateAsync());

        Assert.Null(exception);
        var state = await provider.GetAuthenticationStateAsync();
        Assert.False(state.User.Identity?.IsAuthenticated);
    }
}
