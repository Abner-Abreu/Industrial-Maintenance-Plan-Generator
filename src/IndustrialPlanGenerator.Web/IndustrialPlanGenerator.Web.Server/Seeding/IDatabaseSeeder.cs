namespace IndustrialPlanGenerator.Web.Server.Seeding;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}