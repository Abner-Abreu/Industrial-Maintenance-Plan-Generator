using System.Net.Http.Json;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Web.Client.Models;

namespace IndustrialPlanGenerator.Web.Client.Services;

public class AuthService(HttpClient http, ILocalStorageService storage, JwtAuthStateProvider provider)
{
    public async Task<bool> LoginAsync(LoginDto dto)
    {
        var res = await http.PostAsJsonAsync("api/auth/login", dto);
        if (!res.IsSuccessStatusCode) return false;

        var result = await res.Content.ReadFromJsonAsync<LoginResponse>();
        if (result is null || string.IsNullOrWhiteSpace(result.Token)) return false;

        await storage.SetItemAsStringAsync("authToken", result.Token);
        provider.NotifyStateChanged();
        return true;
    }

    public async Task LogoutAsync()
    {
        await storage.RemoveItemAsync("authToken");
        provider.NotifyStateChanged();
    }
}