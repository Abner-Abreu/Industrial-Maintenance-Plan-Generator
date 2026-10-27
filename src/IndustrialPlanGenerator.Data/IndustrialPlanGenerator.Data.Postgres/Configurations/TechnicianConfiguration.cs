namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.ToTable("technicians");

        builder.HasKey(t => t.Id);

        // PK is also FK to users (1:1, shared PK)
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Technician>(t => t.Id)
            .HasConstraintName("fk_technician_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.Specialty)
            .HasColumnName("specialty")
            .IsRequired()
            .HasMaxLength(200);
    }
}