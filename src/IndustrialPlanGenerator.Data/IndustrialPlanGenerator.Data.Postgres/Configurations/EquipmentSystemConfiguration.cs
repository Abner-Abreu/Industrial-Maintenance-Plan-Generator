namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EquipmentSystemConfiguration : IEntityTypeConfiguration<EquipmentSystem>
{
    public void Configure(EntityTypeBuilder<EquipmentSystem> builder)
    {
        builder.ToTable("equipment_systems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.OperativeState)
            .HasColumnName("operative_state")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LineId)
            .HasColumnName("line_id")
            .IsRequired();

        // RI-06: FK equipment_systems.line_id -> production_lines.id
        builder.HasOne(e => e.ProductionLine)
            .WithMany(l => l.EquipmentSystems)
            .HasForeignKey(e => e.LineId)
            .HasConstraintName("fk_equipment_system_line")
            .OnDelete(DeleteBehavior.Cascade);

        // Relationships
        builder.HasMany(e => e.Sensors)
            .WithOne(s => s.Equipment)
            .HasForeignKey(s => s.EquipmentId)
            .HasConstraintName("fk_sensor_equipment")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.MaintenanceTasks)
            .WithOne(t => t.EquipmentSystem)
            .HasForeignKey(t => t.SystemId)
            .HasConstraintName("fk_maintenance_task_equipment")
            .OnDelete(DeleteBehavior.Restrict);

        // Index for FK
        builder.HasIndex(e => e.LineId)
            .HasDatabaseName("ix_equipment_system_line_id");
    }
}