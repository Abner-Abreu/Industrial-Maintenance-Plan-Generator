namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("shifts");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Schedule)
            .HasColumnName("schedule")
            .IsRequired()
            .HasMaxLength(200);

        // RI-28: Unique constraints on Shift.name and Shift.schedule
        builder.HasIndex(s => s.Name)
            .IsUnique()
            .HasDatabaseName("ux_shift_name");

        builder.HasIndex(s => s.Schedule)
            .IsUnique()
            .HasDatabaseName("ux_shift_schedule");
    }
}