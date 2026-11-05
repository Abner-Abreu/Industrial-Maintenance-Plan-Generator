namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", t =>
        {
            t.HasCheckConstraint("ck_user_role", 
                $"role IN ('{UserRole.Administrator}', '{UserRole.Technician}', '{UserRole.Operator}', '{UserRole.PlantManager}')");
        });

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(u => u.Credentials)
            .HasColumnName("credentials")
            .IsRequired()
            .HasMaxLength(500);
    }
}