namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlantManagerConfiguration : IEntityTypeConfiguration<PlantManager>
{
    public void Configure(EntityTypeBuilder<PlantManager> builder)
    {
        builder.ToTable("plant_managers");

        builder.HasKey(j => j.Id);

        // PK is also FK to users (1:1, shared PK)
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<PlantManager>(j => j.Id)
            .HasConstraintName("fk_plant_manager_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}