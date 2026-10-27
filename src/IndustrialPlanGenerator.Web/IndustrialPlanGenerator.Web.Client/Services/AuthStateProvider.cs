using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;


namespace IndustrialPlanGenerator.Web.Client.Services;

public class JwtAuthStateProvider(ILocalStorageService storage) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anon =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await storage.GetItemAsStringAsync("authToken");
        if (string.IsNullOrWhiteSpace(token)) return Anon;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwt.ValidTo < DateTime.UtcNow) return Anon;

            var identity = new ClaimsIdentity(jwt.Claims, "jwt", "name", "role");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return Anon;
        }
    }

    public void NotifyStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}