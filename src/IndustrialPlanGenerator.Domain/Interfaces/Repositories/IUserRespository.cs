namespace IndustrialPlanGenerator.Domain.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;

public interface IUserRepository
{
    public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);

    Task<bool> IsUserNameExistingAsync(string userName, CancellationToken ct = default);
    Task<UserRole?> GetRoleAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(User User, CancellationToken ct = default);
    void Update(User User);
    void Remove(User User);
}
}