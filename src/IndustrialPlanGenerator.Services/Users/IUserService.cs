using IndustrialPlanGenerator.Services.Contracts.Common;
using IndustrialPlanGenerator.Services.Contracts.Users;

namespace IndustrialPlanGenerator.Services.Users;

/// <summary>User management (USR-1 to USR-4, USR-6 to USR-8).</summary>
public interface IUserService
{
    /// <summary>Lists users with filters and pagination (USR-1). Administrator only (enforced at the endpoint).</summary>
    /// <param name="role">Role to filter by (optional).</param>
    /// <param name="lineId">Only users with that line assigned, as a technician or operator (optional).</param>
    /// <param name="text">Text contained in the name, case-insensitive (optional).</param>
    /// <param name="page">Page, starting at 1. Lower values are corrected to 1.</param>
    /// <param name="pageSize">Page size; limited to 100 and defaults to 20.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Exceptions.InvalidRequestException">The specified role does not exist (HTTP 400).</exception>
    Task<PagedResult<UserSummaryDto>> ListAsync(
        string? role, Guid? lineId, string? text, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns the details of a user (USR-2). Administrators can view any user; other users can view only their own.</summary>
    /// <param name="id">User ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Exceptions.ForbiddenException">Requests another user's details without being an administrator (HTTP 403).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The user does not exist (HTTP 404).</exception>
    Task<UserDetailDto> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates the user and its role record in a single operation (USR-3). Administrator only (enforced at the endpoint).
    /// </summary>
    /// <param name="request">User data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Exceptions.InvalidRequestException">Empty or invalid data, unknown role, or fields that do not correspond to the role (HTTP 400).</exception>
    /// <exception cref="Exceptions.ConflictException">A user with that name already exists (HTTP 409).</exception>
    /// <exception cref="Domain.Exceptions.DomainException">Operator under age or nonexistent shift (HTTP 422).</exception>
    Task<UserDetailDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Edits the name and the data specific to the role (USR-4). Does not change the role or password.
    /// Administrator only (enforced at the endpoint).
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="request">New data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Exceptions.InvalidRequestException">Invalid data or fields that do not correspond to the role (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The user does not exist (HTTP 404).</exception>
    /// <exception cref="Exceptions.ConflictException">Another user already has that name (HTTP 409).</exception>
    /// <exception cref="Domain.Exceptions.DomainException">Operator under age or nonexistent shift (HTTP 422).</exception>
    Task<UserDetailDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Replaces a technician's lines with their permission (USR-6). Changing the lines does not alter already
    /// generated plans. Administrator only (enforced at the endpoint).
    /// </summary>
    /// <param name="id">Technician ID.</param>
    /// <param name="request">New lines with their permission.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Exceptions.InvalidRequestException">Null list, repeated lines, or unknown permission (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The user does not exist (HTTP 404).</exception>
    /// <exception cref="Domain.Exceptions.DomainException">The user is not a technician or some line does not exist (HTTP 422).</exception>
    Task<UserDetailDto> SetTechnicianLinesAsync(Guid id, SetTechnicianLinesRequest request, CancellationToken ct = default);

    /// <summary>
    /// Replaces an operator's lines (USR-7). Administrator only (enforced at the endpoint).
    /// </summary>
    /// <param name="id">Operator ID.</param>
    /// <param name="request">New lines.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Exceptions.InvalidRequestException">Null list or repeated lines (HTTP 400).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The user does not exist (HTTP 404).</exception>
    /// <exception cref="Domain.Exceptions.DomainException">The user is not an operator or some line does not exist (HTTP 422).</exception>
    Task<UserDetailDto> SetOperatorLinesAsync(Guid id, SetOperatorLinesRequest request, CancellationToken ct = default);

    /// <summary>
    /// Lists the technicians authorized on a line (USR-8), for example to choose a support technician
    /// for a reinspection. Administrator and plant manager, on any line; technician and operator, on their own lines.
    /// </summary>
    /// <param name="lineId">Line ID.</param>
    /// <param name="permission">Filters by permission: <c>Generate</c> or <c>Validate</c> (optional).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Exceptions.InvalidRequestException">Unknown permission (HTTP 400).</exception>
    /// <exception cref="Exceptions.ForbiddenException">The user does not have access to that line (HTTP 403).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">The line does not exist (HTTP 404).</exception>
    Task<IReadOnlyList<LineTechnicianDto>> ListLineTechniciansAsync(
        Guid lineId, string? permission, CancellationToken ct = default);
}
