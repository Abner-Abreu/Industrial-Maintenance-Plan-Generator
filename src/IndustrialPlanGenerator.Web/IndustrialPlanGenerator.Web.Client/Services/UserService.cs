using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;

using IndustrialPlanGenerator.Services.Contracts.Common;
using IndustrialPlanGenerator.Services.Contracts.Users;
using IndustrialPlanGenerator.Web.Client.Models;

namespace IndustrialPlanGenerator.Web.Client.Services;

/// <summary>
/// Service responsible for managing users (Administrator role) by communicating with the backend API.
/// </summary>
public class UserService(HttpClient http) : ApiServiceBase
{
    /// <summary>
    /// Retrieves a paginated list of users based on optional filters.
    /// </summary>
    public async Task<ServiceResult<PagedResult<UserSummaryDto>>> GetUsersAsync(
        string? role = null, 
        Guid? lineId = null, 
        string? text = null, 
        int page = 1, 
        int size = 20, 
        string? order = null, 
        string? direction = null)
    {
        try
        {
            var requestUrl = $"api/usuarios?page={page}&size={size}";
            if (!string.IsNullOrWhiteSpace(role)) requestUrl += $"&role={role}";
            if (lineId.HasValue) requestUrl += $"&lineaId={lineId}";
            if (!string.IsNullOrWhiteSpace(text)) requestUrl += $"&texto={text}";
            if (!string.IsNullOrWhiteSpace(order)) requestUrl += $"&orden={order}&dir={direction}";
            
            var response = await http.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<PagedResult<UserSummaryDto>>();
                return ServiceResult<PagedResult<UserSummaryDto>>.Success(data!);
            }

            return await HandleErrorAsync<PagedResult<UserSummaryDto>>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<UserSummaryDto>>.Failure($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves the full details of a specific user.
    /// </summary>
    public async Task<ServiceResult<UserDetailDto>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var response = await http.GetAsync($"api/usuarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<UserDetailDto>();
                return ServiceResult<UserDetailDto>.Success(data!);
            }

            return await HandleErrorAsync<UserDetailDto>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDetailDto>.Failure($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    public async Task<ServiceResult<UserDetailDto>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var response = await http.PostAsJsonAsync("api/usuarios", request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<UserDetailDto>();
                return ServiceResult<UserDetailDto>.Success(data!);
            }

            return await HandleErrorAsync<UserDetailDto>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDetailDto>.Failure($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the basic data of an existing user.
    /// </summary>
    public async Task<ServiceResult<UserDetailDto>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        try
        {
            var response = await http.PutAsJsonAsync($"api/usuarios/{id}", request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<UserDetailDto>();
                return ServiceResult<UserDetailDto>.Success(data!);
            }

            return await HandleErrorAsync<UserDetailDto>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDetailDto>.Failure($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Deactivates a user (logical delete) so they can no longer access the system.
    /// </summary>
    public async Task<ServiceResult<bool>> DeactivateUserAsync(Guid id)
    {
        try
        {
            var response = await http.DeleteAsync($"api/usuarios/{id}");

            if (response.IsSuccessStatusCode)
                return ServiceResult<bool>.Success(true);

            return await HandleErrorAsync<bool>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the lines assigned to a technician, including their specific permissions.
    /// </summary>
    public async Task<ServiceResult<bool>> SetTechnicianLinesAsync(Guid id, SetTechnicianLinesRequest request)
    {
        try
        {
            var response = await http.PutAsJsonAsync($"api/usuarios/{id}/lineas-tecnico", request);
            return response.IsSuccessStatusCode 
                ? ServiceResult<bool>.Success(true) 
                : await HandleErrorAsync<bool>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the lines assigned to an operator.
    /// </summary>
    public async Task<ServiceResult<bool>> SetOperatorLinesAsync(Guid id, SetOperatorLinesRequest request)
    {
        try
        {
            var response = await http.PutAsJsonAsync($"api/usuarios/{id}/lineas-operario", request);
            return response.IsSuccessStatusCode 
                ? ServiceResult<bool>.Success(true) 
                : await HandleErrorAsync<bool>(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    // --- Helper Methods ---

    /// <summary>
    /// Parses the API error response (ProblemDetails) into a user-friendly message.
    /// </summary>
}