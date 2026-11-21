using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

namespace IndustrialPlanGenerator.Web.Client.Services.CatalogsService;

/// <summary>
/// Service for the catalogs page (CAT-1 to CAT-6).
/// Every endpoint requires an authenticated user, and create, update and delete of shifts are
/// administrator-only, so the JWT is attached to every request.
/// Uses the shared contract DTOs directly.
/// </summary>
public sealed class CatalogsService(HttpClient http, ILocalStorageService storage) : ICatalogsService
{
    private const string BaseUrl = "api/catalogs";
    private const string ShiftsUrl = $"{BaseUrl}/shifts";

    // Same key used by AuthService and JwtAuthStateProvider.
    private const string TokenKey = "authToken";

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShiftDto>> LoadShiftsAsync(CancellationToken ct = default)
    {
        return await SendAsync<List<ShiftDto>>(HttpMethod.Get, ShiftsUrl, body: null, ct) ?? [];
    }

    /// <inheritdoc />
    public async Task<ShiftDto> CreateShiftAsync(CreateShiftRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await SendAsync<ShiftDto>(HttpMethod.Post, ShiftsUrl, request, ct)
               ?? throw new InvalidOperationException("Shift creation returned an empty response.");
    }

    /// <inheritdoc />
    public async Task<ShiftDto> UpdateShiftAsync(Guid id, UpdateShiftRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await SendAsync<ShiftDto>(HttpMethod.Put, $"{ShiftsUrl}/{id}", request, ct)
               ?? throw new InvalidOperationException($"Shift {id} update returned an empty response.");
    }

    /// <inheritdoc />
    public async Task<DeleteResult> DeleteShiftAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            // The API answers 204 No Content, so there is no body to read.
            using var response = await SendCoreAsync(HttpMethod.Delete, $"{ShiftsUrl}/{id}", body: null, ct);
            return DeleteResult.Deleted;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return DeleteResult.NotFound;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            return DeleteResult.HasDependencies;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> LoadMaintenanceTypesAsync(CancellationToken ct = default)
    {
        return await SendAsync<List<string>>(HttpMethod.Get, $"{BaseUrl}/maintenance-types", body: null, ct) ?? [];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> LoadCriticalitiesAsync(CancellationToken ct = default)
    {
        return await SendAsync<List<string>>(HttpMethod.Get, $"{BaseUrl}/criticalities", body: null, ct) ?? [];
    }

    /// <summary>
    /// Sends a request and deserializes the JSON response body.
    /// </summary>
    private async Task<T?> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        using var response = await SendCoreAsync(method, url, body, ct);
        return await response.Content.ReadFromJsonAsync<T>(ct);
    }

    /// <summary>
    /// Builds and sends a request with the JWT attached to this request only.
    /// The bearer header is not set on the shared HttpClient so it never leaks into other services (e.g. login).
    /// The caller owns (and must dispose) the returned response.
    /// </summary>
    private async Task<HttpResponseMessage> SendCoreAsync(
        HttpMethod method, string url, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType());
        }

        var token = await storage.GetItemAsStringAsync(TokenKey, ct);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await http.SendAsync(request, ct);

        try
        {
            response.EnsureSuccessStatusCode(); // Throws HttpRequestException with StatusCode set.
            return response;
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }
}