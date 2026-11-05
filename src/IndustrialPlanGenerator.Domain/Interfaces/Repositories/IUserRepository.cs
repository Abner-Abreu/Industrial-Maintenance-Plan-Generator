namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task<bool> IsUserNameExistingAsync(string userName, CancellationToken ct = default);
    Task<UserRole?> GetRoleAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Busca usuarios con filtros opcionales y paginación, ordenados por nombre.
    /// <paramref name="lineId"/> incluye a técnicos y operarios con esa línea asignada;
    /// <paramref name="text"/> busca dentro del nombre sin distinguir mayúsculas.
    /// </summary>
    /// <returns>La página de usuarios y el total de usuarios que cumplen el filtro.</returns>
    Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(
        UserRole? role, Guid? lineId, string? text, int page, int pageSize, CancellationToken ct = default);

    // PlantManager
    Task<PlantManager?> GetPlantManagerAsync(Guid id, CancellationToken ct = default);
    Task AddPlantManagerAsync(PlantManager manager, CancellationToken ct = default);

    // Administrator
    Task<Administrator?> GetAdministratorAsync(Guid id, CancellationToken ct = default);
    Task AddAdministratorAsync(Administrator admin, CancellationToken ct = default);
    
    void Update(User user);
    void Remove(User user);
}