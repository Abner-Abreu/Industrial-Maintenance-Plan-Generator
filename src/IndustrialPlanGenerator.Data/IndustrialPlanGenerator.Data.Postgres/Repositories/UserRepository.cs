namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Data.Postgres;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext db) => _context = db;

    // User
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }
    public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Name == userName, ct);
    }
    public Task<bool> IsUserNameExistingAsync(string userName, CancellationToken ct = default) 
    {
        return _context.Users.AnyAsync(u => u.Name == userName, ct);
    }
    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }
    public void Remove(User user) 
    {
        _context.Users.Remove(user);
    }
    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return _context.Users
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<User>)t.Result, ct);
    }

    public Task<UserRole?> GetRoleAsync(Guid id, CancellationToken ct = default)
    {
        return _context.Users
            .Where(u => u.Id == id)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(ct);
    }

    // PlantManager
    public Task<PlantManager?> GetPlantManagerAsync(Guid id, CancellationToken ct = default)
    {
        return _context.PlantManagers.FirstOrDefaultAsync(pm => pm.Id == id, ct);
    }
    public async Task AddPlantManagerAsync(PlantManager manager, CancellationToken ct = default)
    {
        await _context.PlantManagers.AddAsync(manager, ct);
    }
    // Administrator
    public Task<Administrator?> GetAdministratorAsync(Guid id, CancellationToken ct = default)
    {
        return _context.Administrators.FirstOrDefaultAsync(a => a.Id == id, ct);
    }
    public async Task AddAdministratorAsync(Administrator admin, CancellationToken ct = default)
    {
        await _context.Administrators.AddAsync(admin, ct);
    }
}