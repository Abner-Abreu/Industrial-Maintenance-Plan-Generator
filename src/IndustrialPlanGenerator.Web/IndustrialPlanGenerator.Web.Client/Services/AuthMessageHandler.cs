using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace IndustrialPlanGenerator.Web.Client.Services;

public class AuthMessageHandler(ILocalStorageService storage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await storage.GetItemAsStringAsync("authToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}