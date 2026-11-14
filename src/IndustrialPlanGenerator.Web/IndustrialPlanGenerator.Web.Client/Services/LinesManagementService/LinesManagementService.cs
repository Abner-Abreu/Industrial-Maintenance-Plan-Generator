using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

/// <summary>
/// Service for the lines management page.
/// Phase 1: load only. The list (LIN-1) returns summaries; the detail of a line (LIN-2) is requested separately by ID.
/// Uses the shared contract DTOs directly.
/// </summary>
public sealed class LinesManagementService(HttpClient http, ILocalStorageService storage) : ILinesManagementService
{
    private const string BaseUrl = "api/lines";

    // Same key used by AuthService and JwtAuthStateProvider.
    private const string TokenKey = "authToken";

    /// <inheritdoc />
    public async Task<IReadOnlyList<LineSummaryDto>> LoadLinesAsync(CancellationToken ct = default)
    {
        return await GetAsync<List<LineSummaryDto>>(BaseUrl, ct) ?? [];
    }

    /// <inheritdoc />
    public async Task<LineDetailDto> LoadLineAsync(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<LineDetailDto>($"{BaseUrl}/{id}", ct)
               ?? throw new InvalidOperationException($"Line {id} returned an empty response.");
    }

    /// <summary>
    /// Sends a GET with the JWT attached to this request only.
    /// The bearer header is not set on the shared HttpClient so it never leaks into other services (e.g. login).
    /// </summary>
    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        var token = await storage.GetItemAsStringAsync(TokenKey, ct);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode(); // Throws HttpRequestException with StatusCode set.

        return await response.Content.ReadFromJsonAsync<T>(ct);
    }
}