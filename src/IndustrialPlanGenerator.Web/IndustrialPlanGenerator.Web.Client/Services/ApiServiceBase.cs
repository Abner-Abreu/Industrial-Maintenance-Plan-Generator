using System.Text.Json;
using IndustrialPlanGenerator.Web.Client.Models;

namespace IndustrialPlanGenerator.Web.Client.Services;

/// <summary>
/// Base class for API services, providing shared error handling.
/// </summary>
public abstract class ApiServiceBase
{
    /// <summary>
    /// Parses the API error response (ProblemDetails) into a user-friendly message.
    /// </summary>
    protected static async Task<ServiceResult<T>> HandleErrorAsync<T>(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        
        try
        {
            using var jsonDoc = JsonDocument.Parse(errorContent);
            if (jsonDoc.RootElement.TryGetProperty("detail", out var detail))
                return ServiceResult<T>.Failure(detail.GetString() ?? "An error occurred.");
            if (jsonDoc.RootElement.TryGetProperty("title", out var title))
                return ServiceResult<T>.Failure(title.GetString() ?? "An error occurred.");
        }
        catch
        {
            // Fallback to generic message if the response is not JSON
        }

        return ServiceResult<T>.Failure($"Error {(int)response.StatusCode}: {response.ReasonPhrase}");
    }
}