using System.Net.Http.Json;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Web.Client.Models;

namespace IndustrialPlanGenerator.Web.Client.Services;

/// <summary>
/// Service responsible for retrieving catalogs from the backend API.
/// </summary>
public class CatalogService(HttpClient http) : ApiServiceBase
{
    /// <summary>
    /// Retrieves the list of available work shifts.
    /// </summary>
    public async Task<ServiceResult<List<ShiftDto>>> GetShiftsAsync()
    {
        try
        {
            var response = await http.GetAsync("/api/catalogs/shifts");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<ShiftDto>>();
                return ServiceResult<List<ShiftDto>>.Success(data!);
            }

            return await HandleErrorAsync<List<ShiftDto>>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<ShiftDto>>.Failure($"Network error: {ex.Message}");
        }
    }
}