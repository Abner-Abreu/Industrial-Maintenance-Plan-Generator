namespace IndustrialPlanGenerator.Data.Postgres.Repositories;

using IndustrialPlanGenerator.Domain.Entities;
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
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Name == username, ct);
    }
    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) 
    {
        return _context.Users.AnyAsync(u => u.Name == username, ct);
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