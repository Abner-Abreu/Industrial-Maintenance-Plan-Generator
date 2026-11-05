using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IndustrialPlanGenerator.Data.Postgres;

/// <summary>
/// Factory for creating AppDbContext at design time (e.g., for EF Core CLI tools).
/// Reads configuration from appsettings.json and environment variables.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        var connStr = config.GetConnectionString("Postgres") 
            ?? "Host=localhost;Database=industrial_maintenance;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connStr, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());

        return new AppDbContext(optionsBuilder.Options);
    }
}