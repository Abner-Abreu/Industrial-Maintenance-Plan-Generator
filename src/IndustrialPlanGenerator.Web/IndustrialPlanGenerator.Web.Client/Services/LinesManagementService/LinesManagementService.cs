using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Services.Contracts.Lines;

namespace IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

/// <summary>
/// Service for the lines management page.
/// The list (LIN-1) returns summaries; the detail of a line (LIN-2) is requested separately by ID.
/// Create (LIN-3), update (LIN-4) and delete (LIN-5) of lines, and deletion of systems (SIS-4),
/// are administrator-only operations.
/// Uses the shared contract DTOs directly.
/// </summary>
public sealed class LinesManagementService(HttpClient http, ILocalStorageService storage) : ILinesManagementService
{
    private const string BaseUrl = "api/lines";
    private const string SystemsUrl = "api/systems";

    // Same key used by AuthService and JwtAuthStateProvider.
    private const string TokenKey = "authToken";

    /// <inheritdoc />
    public async Task<IReadOnlyList<LineSummaryDto>> LoadLinesAsync(CancellationToken ct = default)
    {
        return await SendAsync<List<LineSummaryDto>>(HttpMethod.Get, BaseUrl, body: null, ct) ?? [];
    }

    /// <inheritdoc />
    public async Task<LineDetailDto> LoadLineAsync(Guid id, CancellationToken ct = default)
    {
        return await SendAsync<LineDetailDto>(HttpMethod.Get, $"{BaseUrl}/{id}", body: null, ct)
               ?? throw new InvalidOperationException($"Line {id} returned an empty response.");
    }

    /// <inheritdoc />
    public async Task<LineDetailDto> CreateLineAsync(CreateLineRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await SendAsync<LineDetailDto>(HttpMethod.Post, BaseUrl, request, ct)
               ?? throw new InvalidOperationException("Line creation returned an empty response.");
    }

    /// <inheritdoc />
    public async Task<LineDetailDto> UpdateLineAsync(Guid id, UpdateLineRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await SendAsync<LineDetailDto>(HttpMethod.Put, $"{BaseUrl}/{id}", request, ct)
               ?? throw new InvalidOperationException($"Line {id} update returned an empty response.");
    }

    /// <inheritdoc />
    public Task<DeleteResult> DeleteLineAsync(Guid id, CancellationToken ct = default)
        => DeleteAsync($"{BaseUrl}/{id}", ct);

    /// <inheritdoc />
    public Task<DeleteResult> DeleteSystemAsync(Guid systemId, CancellationToken ct = default)
        => DeleteAsync($"{SystemsUrl}/{systemId}", ct);

    /// <summary>
    /// Sends a DELETE and translates the expected 404 and 409 responses into a <see cref="DeleteResult"/>.
    /// Any other failure throws.
    /// </summary>
    private async Task<DeleteResult> DeleteAsync(string url, CancellationToken ct)
    {
        try
        {
            // The API answers 204 No Content, so there is no body to read.
            using var response = await SendCoreAsync(HttpMethod.Delete, url, body: null, ct);
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