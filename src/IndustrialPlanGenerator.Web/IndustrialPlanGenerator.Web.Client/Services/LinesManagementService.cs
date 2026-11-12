using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Web.Client.Services;

/// <summary>
/// Service for the lines management page.
/// Phase 1: load only. Fetches the list (LIN-1) and, using the returned IDs, completes each line with its detail (LIN-2).
/// Uses the shared contract DTOs directly.
/// </summary>
public sealed class LinesManagementService(HttpClient http, ILocalStorageService storage)
{
    private const string BaseUrl = "api/lines";

    // Same key used by AuthService and JwtAuthStateProvider.
    private const string TokenKey = "authToken";

    /// <summary>Loads all visible lines together with their systems.</summary>
    public async Task<IReadOnlyList<LineDetailDto>> LoadLinesAsync(CancellationToken ct = default)
    {
        var summaries = await GetAsync<List<LineSummaryDto>>(BaseUrl, ct) ?? [];

        // One detail GET per ID from the list, executed in parallel.
        return await Task.WhenAll(summaries.Select(summary => LoadLineAsync(summary.Id, ct)));
    }

    /// <summary>Loads the detail of a single line.</summary>
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