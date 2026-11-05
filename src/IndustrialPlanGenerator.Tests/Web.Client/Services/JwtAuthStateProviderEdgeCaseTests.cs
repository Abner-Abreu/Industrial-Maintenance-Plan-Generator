using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Web.Client.Services;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace IndustrialPlanGenerator.Tests.Web.Client.Services;

/// <summary>
/// Edge cases for JwtAuthStateProvider. Several of these document deliberate decisions
/// (for example: the client does NOT validate signatures).
/// </summary>
public class JwtAuthStateProviderEdgeCaseTests
{
    // ----- helpers (private to this class to avoid clashing with other test files) -----

    private static string CreateToken(string signingKey, DateTime? expiresUtc, params Claim[] claims)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: expiresUtc?.AddHours(-2),
            expires: expiresUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static Claim[] DefaultClaims() =>
    [
        new Claim("sub", Guid.NewGuid().ToString()),
        new Claim("name", "tuto"),
        new Claim("role", "Admin")
    ];

    private static (JwtAuthStateProvider Provider, Mock<ILocalStorageService> Storage) Build(string? token)
    {
        var storage = new Mock<ILocalStorageService>();
        storage
            .Setup(s => s.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        return (new JwtAuthStateProvider(storage.Object), storage);
    }

    // ----- tests -----

    [Fact]
    public async Task TokenWithoutExpClaim_IsTreatedAsExpired()
    {
        // Without "exp", JwtSecurityToken.ValidTo is DateTime.MinValue, so the token counts as expired.
        // The backend always sets "exp"; this test makes the decision explicit.
        var token = CreateToken("key-key-key-key-key-key-key-key-1234", expiresUtc: null, DefaultClaims());
        var (provider, _) = Build(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task ClientDoesNotValidateSignature_ForgedTokenLooksAuthenticatedInTheUi()
    {
        // IMPORTANT: this documents a known limitation, not a feature.
        // The client only READS the token to decide what to render. Real security is the API's job
        // ([Authorize] + signature validation). A forged token must never unlock server data.
        var forged = CreateToken(
            "attacker-key-attacker-key-attacker-key-1234",
            DateTime.UtcNow.AddHours(1),
            DefaultClaims());
        var (provider, _) = Build(forged);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task TokenThatJustExpired_IsAnonymous()
    {
        var token = CreateToken(
            "key-key-key-key-key-key-key-key-1234",
            DateTime.UtcNow.AddSeconds(-5),
            DefaultClaims());
        var (provider, _) = Build(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task TokenThatExpiresSoon_IsStillAuthenticated()
    {
        var token = CreateToken(
            "key-key-key-key-key-key-key-key-1234",
            DateTime.UtcNow.AddMinutes(5),
            DefaultClaims());
        var (provider, _) = Build(token);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.Identity?.IsAuthenticated);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("a.b")]
    [InlineData("a.b.c")]
    [InlineData("...")]
    [InlineData("not a token at all")]
    public async Task VariousMalformedTokens_ReturnAnonymousAndDoNotThrow(string malformed)
    {
        var (provider, _) = Build(malformed);

        var exception = await Record.ExceptionAsync(() => provider.GetAuthenticationStateAsync());

        Assert.Null(exception);
        var state = await provider.GetAuthenticationStateAsync();
        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task StateReflectsStorageOnEveryCall_RemovingTheTokenLogsTheUserOut()
    {
        var validToken = CreateToken(
            "key-key-key-key-key-key-key-key-1234",
            DateTime.UtcNow.AddHours(1),
            DefaultClaims());
        var storage = new Mock<ILocalStorageService>();
        var current = (string?)validToken;
        storage
            .Setup(s => s.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => current);
        var provider = new JwtAuthStateProvider(storage.Object);

        var before = await provider.GetAuthenticationStateAsync();
        current = null; // simulates the token being removed from localStorage
        var after = await provider.GetAuthenticationStateAsync();

        Assert.True(before.User.Identity?.IsAuthenticated);
        Assert.False(after.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task NotifyStateChanged_PublishesTheCurrentState()
    {
        var token = CreateToken(
            "key-key-key-key-key-key-key-key-1234",
            DateTime.UtcNow.AddHours(1),
            DefaultClaims());
        var (provider, _) = Build(token);
        Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState>? published = null;
        provider.AuthenticationStateChanged += task => published = task;

        provider.NotifyStateChanged();

        Assert.NotNull(published);
        var state = await published!;
        Assert.True(state.User.Identity?.IsAuthenticated);
        Assert.Equal("tuto", state.User.Identity?.Name);
    }
}
