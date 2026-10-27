using System.Net;
using System.Text;
using Blazored.LocalStorage;
using IndustrialPlanGenerator.Web.Client.Models;
using IndustrialPlanGenerator.Web.Client.Services;
using Moq;

namespace IndustrialPlanGenerator.Tests.Web.Client.Services;

/// <summary>
/// Behavior tests for the client-side AuthService: what it sends, what it stores,
/// and whether it notifies the auth state provider so the UI refreshes.
/// </summary>
public class AuthServiceBehaviorTests
{
    // ----- helpers (private to this class to avoid clashing with other test files) -----

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public int CallCount { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastBody { get; private set; }

        public StubHandler(HttpStatusCode status, string body)
            : this(_ => new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            })
        {
        }

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            CallCount++;
            LastRequest = request;
            LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return _respond(request);
        }
    }

    private sealed class Sut
    {
        public required AuthService Service { get; init; }
        public required JwtAuthStateProvider Provider { get; init; }
        public required Mock<ILocalStorageService> Storage { get; init; }
        public required StubHandler Handler { get; init; }
        public int StateChangedCount;
    }

    private static Sut Build(StubHandler handler)
    {
        var storage = new Mock<ILocalStorageService>();
        var provider = new JwtAuthStateProvider(storage.Object);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5101/") };

        var sut = new Sut
        {
            Service = new AuthService(http, storage.Object, provider),
            Provider = provider,
            Storage = storage,
            Handler = handler
        };

        provider.AuthenticationStateChanged += _ => sut.StateChangedCount++;
        return sut;
    }

    private static LoginDto ValidCredentials() => new() { Username = "tuto", Password = "1234" };

    // ----- tests -----

    [Fact]
    public async Task Login_Success_NotifiesAuthStateChangedExactlyOnce()
    {
        var sut = Build(new StubHandler(HttpStatusCode.OK, """{"token":"a.b.c"}"""));

        await sut.Service.LoginAsync(ValidCredentials());

        Assert.Equal(1, sut.StateChangedCount);
    }

    [Fact]
    public async Task Login_Failure_DoesNotNotifyAuthStateChanged()
    {
        var sut = Build(new StubHandler(HttpStatusCode.Unauthorized, "{}"));

        await sut.Service.LoginAsync(ValidCredentials());

        Assert.Equal(0, sut.StateChangedCount);
    }

    [Fact]
    public async Task Login_ParsesCamelCaseTokenFromBackendResponse()
    {
        // The backend serializes with camelCase; the client must read it without extra attributes.
        var sut = Build(new StubHandler(HttpStatusCode.OK, """{"token":"header.payload.signature"}"""));

        var ok = await sut.Service.LoginAsync(ValidCredentials());

        Assert.True(ok);
        sut.Storage.Verify(s => s.SetItemAsStringAsync(
            "authToken", "header.payload.signature", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_IgnoresUnknownFieldsInResponse()
    {
        // The backend also returns expiresAt and user; extra fields must not break deserialization.
        const string body = """
            {
              "token": "a.b.c",
              "expiresAt": "2026-10-27T15:00:00+00:00",
              "user": { "id": "5f0c2d1e-0000-0000-0000-000000000001", "name": "tuto", "role": "Admin" }
            }
            """;
        var sut = Build(new StubHandler(HttpStatusCode.OK, body));

        var ok = await sut.Service.LoginAsync(ValidCredentials());

        Assert.True(ok);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Login_AnyNonSuccessStatus_ReturnsFalseAndStoresNothing(HttpStatusCode status)
    {
        var sut = Build(new StubHandler(status, "{}"));

        var ok = await sut.Service.LoginAsync(ValidCredentials());

        Assert.False(ok);
        sut.Storage.Verify(s => s.SetItemAsStringAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_SendsExactlyOneRequest()
    {
        var sut = Build(new StubHandler(HttpStatusCode.OK, """{"token":"a.b.c"}"""));

        await sut.Service.LoginAsync(ValidCredentials());

        Assert.Equal(1, sut.Handler.CallCount);
    }

    [Fact]
    public async Task Login_DoesNotSendTheStoredTokenOrAuthorizationHeader()
    {
        // Login is an anonymous endpoint: it must not carry a Bearer header.
        var sut = Build(new StubHandler(HttpStatusCode.OK, """{"token":"a.b.c"}"""));

        await sut.Service.LoginAsync(ValidCredentials());

        Assert.Null(sut.Handler.LastRequest!.Headers.Authorization);
    }

    [Fact]
    public async Task Login_BodyIsJson()
    {
        var sut = Build(new StubHandler(HttpStatusCode.OK, """{"token":"a.b.c"}"""));

        await sut.Service.LoginAsync(ValidCredentials());

        Assert.Equal("application/json", sut.Handler.LastRequest!.Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Login_WithNonJsonSuccessBody_ThrowsInsteadOfReturningTrue()
    {
        // Documents current behavior: a 200 with garbage body surfaces as an exception.
        // If you decide it should return false instead, change this test together with the service.
        var sut = Build(new StubHandler(HttpStatusCode.OK, "<html>proxy error</html>"));

        await Assert.ThrowsAnyAsync<Exception>(() => sut.Service.LoginAsync(ValidCredentials()));

        sut.Storage.Verify(s => s.SetItemAsStringAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Logout_RemovesTokenAndNotifiesAuthStateChanged()
    {
        var sut = Build(new StubHandler(HttpStatusCode.OK, "{}"));

        await sut.Service.LogoutAsync();

        sut.Storage.Verify(s => s.RemoveItemAsync("authToken", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1, sut.StateChangedCount);
    }

    [Fact]
    public async Task Logout_DoesNotCallTheApi()
    {
        var sut = Build(new StubHandler(HttpStatusCode.OK, "{}"));

        await sut.Service.LogoutAsync();

        Assert.Equal(0, sut.Handler.CallCount);
    }
}
