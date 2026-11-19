using System.Net.Http.Json;
using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Web.Client.Models;

namespace IndustrialPlanGenerator.Web.Client.Services;

/// <summary>
/// Service responsible for retrieving production lines from the backend API.
/// </summary>
public class LineService(HttpClient http) : ApiServiceBase
{
    /// <summary>
    /// Retrieves the list of all production lines.
    /// </summary>
    public async Task<ServiceResult<List<LineSummaryDto>>> GetLinesAsync()
    {
        try
        {
            var response = await http.GetAsync("/api/lines");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<LineSummaryDto>>();
                return ServiceResult<List<LineSummaryDto>>.Success(data!);
            }

            return await HandleErrorAsync<List<LineSummaryDto>>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<LineSummaryDto>>.Failure($"Network error: {ex.Message}");
        }
    }
}