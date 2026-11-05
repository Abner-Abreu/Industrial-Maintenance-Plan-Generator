using IndustrialPlanGenerator.Services.Contracts.Common;
using IndustrialPlanGenerator.Services.Contracts.Users;

namespace IndustrialPlanGenerator.Services.Users;

/// <summary>Gestión de usuarios (USR-1 a USR-3).</summary>
public interface IUserService
{
    /// <summary>Lista usuarios con filtros y paginación (USR-1). Solo administrador (se exige en el endpoint).</summary>
    /// <param name="role">Rol por el que filtrar (opcional).</param>
    /// <param name="lineId">Solo usuarios con esa línea asignada, como técnico u operario (opcional).</param>
    /// <param name="text">Texto contenido en el nombre, sin distinguir mayúsculas (opcional).</param>
    /// <param name="page">Página, desde 1. Un valor menor se corrige a 1.</param>
    /// <param name="pageSize">Tamaño de página; se limita a 100 y por defecto es 20.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="Exceptions.InvalidRequestException">El rol indicado no existe (HTTP 400).</exception>
    Task<PagedResult<UserSummaryDto>> ListAsync(
        string? role, Guid? lineId, string? text, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Devuelve el detalle de un usuario (USR-2). El administrador ve cualquiera; el resto, solo el suyo.</summary>
    /// <param name="id">Id del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="Exceptions.ForbiddenException">Pide el detalle de otro usuario sin ser administrador (HTTP 403).</exception>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">No existe el usuario (HTTP 404).</exception>
    Task<UserDetailDto> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Crea el usuario y su registro de rol en una sola operación (USR-3). Solo administrador (se exige en el endpoint).
    /// </summary>
    /// <param name="request">Datos del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <exception cref="Exceptions.InvalidRequestException">Datos vacíos o inválidos, rol desconocido, o campos que no corresponden al rol (HTTP 400).</exception>
    /// <exception cref="Exceptions.ConflictException">Ya existe un usuario con ese nombre (HTTP 409).</exception>
    /// <exception cref="Domain.Exceptions.DomainException">Operario menor de edad o turno inexistente (HTTP 422).</exception>
    Task<UserDetailDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
}
