using IndustrialPlanGenerator.Data.Postgres;
using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace IndustrialPlanGenerator.Web.Server.Seeding;

public sealed class DatabaseSeeder : IDatabaseSeeder
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DatabaseSeeder> _logger;

    // Fixed seed user IDs for consistency across environments
    private static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TechnicianUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OperatorUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid PlantManagerUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid ShiftMorningId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private static readonly Guid ShiftAfternoonId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    private static readonly Guid ShiftNightId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid Line1Id = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid Line2Id = Guid.Parse("22222222-3333-4444-5555-666666666666");
    private static readonly Guid SupportTechUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public DatabaseSeeder(IServiceProvider services, ILogger<DatabaseSeeder> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Ensure database is created and migrations applied
        await context.Database.MigrateAsync(ct);

        // Seed in dependency order
        await SeedShiftsAsync(context, ct);
        await SeedLinesAsync(context, ct);
        await SeedUsersAndRolesAsync(context, hasher, ct);
        await SeedTechnicianLinesAsync(context, ct);
        await SeedOperatorLinesAsync(context, ct);

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Database seeding completed");
    }

    private static async Task SeedShiftsAsync(AppDbContext context, CancellationToken ct)
    {
        if (await context.Shifts.AnyAsync(ct)) return;

        var shifts = new[]
        {
            new Shift { Id = ShiftMorningId, Name = "Morning", Schedule = "06:00-14:00", Operators = new List<Operator>() },
            new Shift { Id = ShiftAfternoonId, Name = "Afternoon", Schedule = "14:00-22:00", Operators = new List<Operator>() },
            new Shift { Id = ShiftNightId, Name = "Night", Schedule = "22:00-06:00", Operators = new List<Operator>() }
        };
        await context.Shifts.AddRangeAsync(shifts, ct);
    }

    private static async Task SeedLinesAsync(AppDbContext context, CancellationToken ct)
    {
        if (await context.ProductionLines.AnyAsync(ct)) return;

        var lines = new[]
        {
            new ProductionLine
            {
                Id = Line1Id,
                Name = "Line A - Assembly",
                ProductiveProcess = "Automotive assembly line",
                EquipmentSystems = new List<EquipmentSystem>(),
                TechnicianLines = new List<TechnicianLine>(),
                Operators = new List<OperatorLine>(),
                MaintenancePlans = new List<MaintenancePlan>()
            },
            new ProductionLine
            {
                Id = Line2Id,
                Name = "Line B - Packaging",
                ProductiveProcess = "Product packaging and labeling",
                EquipmentSystems = new List<EquipmentSystem>(),
                TechnicianLines = new List<TechnicianLine>(),
                Operators = new List<OperatorLine>(),
                MaintenancePlans = new List<MaintenancePlan>()
            }
        };
        await context.ProductionLines.AddRangeAsync(lines, ct);
    }

    private static async Task SeedUsersAndRolesAsync(AppDbContext context, IPasswordHasher hasher, CancellationToken ct)
    {
        if (await context.Users.AnyAsync(ct)) return;

        var password = "Test123!";
        var passwordHash = hasher.Hash(password);

        var users = new[]
        {
            // Administrator
            new User
            {
                Id = AdminUserId,
                Name = "admin",
                Role = UserRole.Administrator,
                Credentials = passwordHash
            },
            // Technician
            new User
            {
                Id = TechnicianUserId,
                Name = "technician",
                Role = UserRole.Technician,
                Credentials = passwordHash
            },
            // Operator
            new User
            {
                Id = OperatorUserId,
                Name = "operator",
                Role = UserRole.Operator,
                Credentials = passwordHash
            },
            // Plant Manager
            new User
            {
                Id = PlantManagerUserId,
                Name = "plantmanager",
                Role = UserRole.PlantManager,
                Credentials = passwordHash
            },
            // Support Technician
            new User
            {
                Id = SupportTechUserId,
                Name = "supporttech",
                Role = UserRole.Technician,
                Credentials = passwordHash
            }
        };

        await context.Users.AddRangeAsync(users, ct);

        // Subtype entities (shared PK with User)
        await context.Administrators.AddAsync(new Administrator
        {
            Id = AdminUserId
        }, ct);

        await context.Technicians.AddAsync(new Technician
        {
            Id = TechnicianUserId,
            Specialty = "Mechanical maintenance",
            Lines = new List<TechnicianLine>(),
            MaintenancePlans = new List<MaintenancePlan>(),
            MaintenanceTasks = new List<MaintenanceTask>(),
            ReinspectionRequests = new List<ReinspectionRequest>()
        }, ct);

        await context.PlantManagers.AddAsync(new PlantManager
        {
            Id = PlantManagerUserId,
            PlanValidations = new List<PlanValidation>()
        }, ct);

        await context.Operators.AddAsync(new Operator
        {
            Id = OperatorUserId,
            Age = 30,
            ShiftId = ShiftMorningId,
            Shift = null!, // Will be set by FK
            Lines = new List<OperatorLine>(),
            Plans = new List<PlanAssignment>(),
            ReinspectionRequests = new List<ReinspectionRequest>()
        }, ct);

        await context.Technicians.AddAsync(new Technician
        {
            Id = SupportTechUserId,
            Specialty = "Electrical systems",
            Lines = new List<TechnicianLine>(),
            MaintenancePlans = new List<MaintenancePlan>(),
            MaintenanceTasks = new List<MaintenanceTask>(),
            ReinspectionRequests = new List<ReinspectionRequest>()
        }, ct);
    }

    private static async Task SeedTechnicianLinesAsync(AppDbContext context, CancellationToken ct)
    {
        if (await context.TechnicianLines.AnyAsync(ct)) return;

        var techLines = new[]
        {
            new TechnicianLine
            {
                TechnicianId = TechnicianUserId,
                LineId = Line1Id,
                Permission = TechnicianPermission.Generate,
                Technician = null!,
                Line = null!
            },
            new TechnicianLine
            {
                TechnicianId = TechnicianUserId,
                LineId = Line2Id,
                Permission = TechnicianPermission.Validate,
                Technician = null!,
                Line = null!
            },
            new TechnicianLine
            {
                TechnicianId = SupportTechUserId,
                LineId = Line1Id,
                Permission = TechnicianPermission.Generate,
                Technician = null!,
                Line = null!
            }
        };
        await context.TechnicianLines.AddRangeAsync(techLines, ct);
    }

    private static async Task SeedOperatorLinesAsync(AppDbContext context, CancellationToken ct)
    {
        if (await context.OperatorLines.AnyAsync(ct)) return;

        var opLines = new[]
        {
            new OperatorLine
            {
                OperatorId = OperatorUserId,
                LineId = Line1Id,
                Operator = null!,
                Line = null!
            },
            new OperatorLine
            {
                OperatorId = OperatorUserId,
                LineId = Line2Id,
                Operator = null!,
                Line = null!
            }
        };
        await context.OperatorLines.AddRangeAsync(opLines, ct);
    }
}