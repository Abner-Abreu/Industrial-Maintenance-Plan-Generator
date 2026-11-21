using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;
using Sut = IndustrialPlanGenerator.Web.Client.Services.CatalogsService.CatalogsService;

namespace IndustrialPlanGenerator.Tests.Web.Client.Services.Catalogs;

/// <summary>
/// Checks that <c>CatalogsService</c> talks to the catalogs API as the contract describes:
/// right HTTP method and URL, request body, JWT header, response mapping and error handling.
/// The network is replaced by a stub handler and the local storage by a substitute,
/// so no server or browser is needed.
/// </summary>
public sealed class CatalogsServiceTests
{
    private const string TokenKey = "authToken";
    private const string Token = "test-jwt";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly Guid ShiftId = Guid.Parse("a1b2c3d4-0001-4000-8000-000000000001");

    // ---- Authentication --------------------------------------------------

    [Fact]
    public async Task EveryRequest_CarriesTheStoredToken_AsBearer()
    {
        var (service, handler) = CreateSut(HttpStatusCode.OK, Array.Empty<ShiftDto>());

        await service.LoadShiftsAsync();

        Assert.Equal(new AuthenticationHeaderValue("Bearer", Token), handler.Authorization);
    }

    [Fact]
    public async Task Post_CarriesTheStoredToken_AsBearer()
    {
        var created = new ShiftDto(ShiftId, "Morning", "06:00-14:00");
        var (service, handler) = CreateSut(HttpStatusCode.Created, created);

        await service.CreateShiftAsync(new CreateShiftRequest("Morning", "06:00-14:00"));

        Assert.Equal(new AuthenticationHeaderValue("Bearer", Token), handler.Authorization);
    }

    [Fact]
    public async Task Delete_CarriesTheStoredToken_AsBearer()
    {
        var (service, handler) = CreateSut(HttpStatusCode.NoContent);

        await service.DeleteShiftAsync(ShiftId);

        Assert.Equal(new AuthenticationHeaderValue("Bearer", Token), handler.Authorization);
    }

    [Fact]
    public async Task WithoutStoredToken_NoAuthorizationHeaderIsSent()
    {
        var (service, handler) = CreateSut(HttpStatusCode.OK, Array.Empty<ShiftDto>(), token: null);

        await service.LoadShiftsAsync();

        Assert.Null(handler.Authorization);
    }

    // ---- CAT-1: list shifts ----------------------------------------------

    [Fact]
    public async Task LoadShifts_CallsGetOnShiftsRoute_AndReturnsTheShifts()
    {
        var expected = new[] { new ShiftDto(ShiftId, "Morning", "06:00-14:00") };
        var (service, handler) = CreateSut(HttpStatusCode.OK, expected);

        var shifts = await service.LoadShiftsAsync();

        Assert.Equal(HttpMethod.Get, handler.Method);
        Assert.Equal("/api/catalogs/shifts", handler.Path);
        Assert.Equal(expected, shifts);
    }

    [Fact]
    public async Task LoadShifts_WhenBodyIsNull_ReturnsEmptyList()
    {
        var (service, _) = CreateSutRaw(HttpStatusCode.OK, "null");

        var shifts = await service.LoadShiftsAsync();

        Assert.Empty(shifts);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task LoadShifts_OnFailure_ThrowsWithStatusCode(HttpStatusCode status)
    {
        var (service, _) = CreateSut(status);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.LoadShiftsAsync());

        Assert.Equal(status, ex.StatusCode);
    }

    // ---- CAT-2: create shift ---------------------------------------------

    [Fact]
    public async Task CreateShift_PostsTheRequest_AndReturnsTheCreatedShift()
    {
        var request = new CreateShiftRequest("Morning", "06:00-14:00");
        var created = new ShiftDto(ShiftId, "Morning", "06:00-14:00");
        var (service, handler) = CreateSut(HttpStatusCode.Created, created);

        var result = await service.CreateShiftAsync(request);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/api/catalogs/shifts", handler.Path);
        Assert.Equal(request, JsonSerializer.Deserialize<CreateShiftRequest>(handler.Body!, Json));
        Assert.Equal(created, result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task CreateShift_OnFailure_ThrowsWithStatusCode(HttpStatusCode status)
    {
        var (service, _) = CreateSut(status);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => service.CreateShiftAsync(new CreateShiftRequest("Morning", "06:00-14:00")));

        Assert.Equal(status, ex.StatusCode);
    }

    [Fact]
    public async Task CreateShift_WhenSuccessHasNoBody_Throws()
    {
        var (service, _) = CreateSutRaw(HttpStatusCode.Created, "null");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateShiftAsync(new CreateShiftRequest("Morning", "06:00-14:00")));
    }

    // ---- CAT-3: update shift ---------------------------------------------

    [Fact]
    public async Task UpdateShift_PutsTheRequestOnTheShiftRoute_AndReturnsTheUpdatedShift()
    {
        var request = new UpdateShiftRequest("Early", "05:00-13:00");
        var updated = new ShiftDto(ShiftId, "Early", "05:00-13:00");
        var (service, handler) = CreateSut(HttpStatusCode.OK, updated);

        var result = await service.UpdateShiftAsync(ShiftId, request);

        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Equal($"/api/catalogs/shifts/{ShiftId}", handler.Path);
        Assert.Equal(request, JsonSerializer.Deserialize<UpdateShiftRequest>(handler.Body!, Json));
        Assert.Equal(updated, result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    public async Task UpdateShift_OnFailure_ThrowsWithStatusCode(HttpStatusCode status)
    {
        var (service, _) = CreateSut(status);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => service.UpdateShiftAsync(ShiftId, new UpdateShiftRequest("Early", "05:00-13:00")));

        Assert.Equal(status, ex.StatusCode);
    }

    // ---- CAT-4: delete shift ---------------------------------------------

    [Fact]
    public async Task DeleteShift_On204_ReturnsDeleted()
    {
        var (service, handler) = CreateSut(HttpStatusCode.NoContent);

        var result = await service.DeleteShiftAsync(ShiftId);

        Assert.Equal(HttpMethod.Delete, handler.Method);
        Assert.Equal($"/api/catalogs/shifts/{ShiftId}", handler.Path);
        Assert.Equal(DeleteResult.Deleted, result);
    }

    [Fact]
    public async Task DeleteShift_On404_ReturnsNotFound()
    {
        var (service, _) = CreateSut(HttpStatusCode.NotFound);

        Assert.Equal(DeleteResult.NotFound, await service.DeleteShiftAsync(ShiftId));
    }

    [Fact]
    public async Task DeleteShift_On409_ReturnsHasDependencies()
    {
        var (service, _) = CreateSut(HttpStatusCode.Conflict);

        Assert.Equal(DeleteResult.HasDependencies, await service.DeleteShiftAsync(ShiftId));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task DeleteShift_OnUnexpectedFailure_Throws(HttpStatusCode status)
    {
        var (service, _) = CreateSut(status);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.DeleteShiftAsync(ShiftId));

        Assert.Equal(status, ex.StatusCode);
    }

    // ---- CAT-5 and CAT-6: fixed catalogs ---------------------------------

    [Fact]
    public async Task LoadMaintenanceTypes_CallsGetOnItsRoute_AndReturnsTheValues()
    {
        var expected = new[] { "Preventive", "Corrective", "Predictive" };
        var (service, handler) = CreateSut(HttpStatusCode.OK, expected);

        var types = await service.LoadMaintenanceTypesAsync();

        Assert.Equal(HttpMethod.Get, handler.Method);
        Assert.Equal("/api/catalogs/maintenance-types", handler.Path);
        Assert.Equal(expected, types);
    }

    [Fact]
    public async Task LoadCriticalities_CallsGetOnItsRoute_AndReturnsTheValues()
    {
        var expected = new[] { "Low", "Medium", "High" };
        var (service, handler) = CreateSut(HttpStatusCode.OK, expected);

        var criticalities = await service.LoadCriticalitiesAsync();

        Assert.Equal(HttpMethod.Get, handler.Method);
        Assert.Equal("/api/catalogs/criticalities", handler.Path);
        Assert.Equal(expected, criticalities);
    }

    [Fact]
    public async Task LoadFixedCatalogs_OnFailure_Throws()
    {
        var (service, _) = CreateSut(HttpStatusCode.Unauthorized);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.LoadMaintenanceTypesAsync());
        await Assert.ThrowsAsync<HttpRequestException>(() => service.LoadCriticalitiesAsync());
    }

    // ---- Cancellation ----------------------------------------------------

    [Fact]
    public async Task WhenTokenIsAlreadyCancelled_NoRequestIsSent()
    {
        var (service, handler) = CreateSut(HttpStatusCode.OK, Array.Empty<ShiftDto>());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.LoadShiftsAsync(cts.Token));

        Assert.Equal(0, handler.Calls);
    }

    // ---- Helpers ---------------------------------------------------------

    /// <summary>Builds the service over a stub that answers with <paramref name="status"/> and a JSON body.</summary>
    private static (Sut Service, StubHandler Handler) CreateSut(
        HttpStatusCode status, object? body = null, string? token = Token)
        => Build(() => new HttpResponseMessage(status)
        {
            Content = body is null ? null : JsonContent.Create(body, options: Json)
        }, token);

    /// <summary>Same as <see cref="CreateSut"/> but with a raw JSON text as the body.</summary>
    private static (Sut Service, StubHandler Handler) CreateSutRaw(
        HttpStatusCode status, string rawJson, string? token = Token)
        => Build(() => new HttpResponseMessage(status)
        {
            Content = new StringContent(rawJson, Encoding.UTF8, "application/json")
        }, token);

    private static (Sut Service, StubHandler Handler) Build(Func<HttpResponseMessage> respond, string? token)
    {
        var storage = DispatchProxy.Create<ILocalStorageService, StorageFake>();
        ((StorageFake)(object)storage).Token = token;

        var handler = new StubHandler(respond);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") };
        return (new Sut(http, storage), handler);
    }

    /// <summary>
    /// Minimal stand-in for <see cref="ILocalStorageService"/> that needs no mocking library.
    /// Only reading the auth token is supported; any other member throws.
    /// Public because <see cref="DispatchProxy"/> must be able to derive from it.
    /// </summary>
    public class StorageFake : DispatchProxy
    {
        public string? Token { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(ILocalStorageService.GetItemAsStringAsync)
                && args is { Length: > 0 } && Equals(args[0], TokenKey))
            {
                return new ValueTask<string>(Token!);
            }

            throw new NotSupportedException($"{targetMethod?.Name} is not used by these tests.");
        }
    }

    /// <summary>Replaces the network: records the last request and returns a canned response.</summary>
    private sealed class StubHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? Path { get; private set; }
        public string? Body { get; private set; }
        public AuthenticationHeaderValue? Authorization { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            Calls++;
            Method = request.Method;
            Path = request.RequestUri?.AbsolutePath;
            Authorization = request.Headers.Authorization;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);

            return respond();
        }
    }
}