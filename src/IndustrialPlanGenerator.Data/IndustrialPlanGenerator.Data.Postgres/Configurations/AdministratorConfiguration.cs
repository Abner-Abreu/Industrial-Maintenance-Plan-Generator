namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> builder)
    {
        // TPT (Table Per Type) - separate table with PK = FK to users
        builder.ToTable("administrators");

        builder.HasKey(a => a.Id);

        // PK is also FK to users (1:1, shared PK)
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Administrator>(a => a.Id)
            .HasConstraintName("fk_administrator_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}