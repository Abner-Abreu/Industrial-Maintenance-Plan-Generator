namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductionLineConfiguration : IEntityTypeConfiguration<ProductionLine>
{
    public void Configure(EntityTypeBuilder<ProductionLine> builder)
    {
        builder.ToTable("production_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.ProductiveProcess)
            .HasColumnName("productive_process")
            .IsRequired()
            .HasMaxLength(500);

        // Relationships
        builder.HasMany(l => l.EquipmentSystems)
            .WithOne(e => e.ProductionLine)
            .HasForeignKey(e => e.LineId)
            .HasConstraintName("fk_equipment_system_line")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.TechnicianLines)
            .WithOne(tl => tl.Line)
            .HasForeignKey(tl => tl.LineId)
            .HasConstraintName("fk_technician_line_line")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Operators)
            .WithOne(ol => ol.Line)
            .HasForeignKey(ol => ol.LineId)
            .HasConstraintName("fk_operator_line_line")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.MaintenancePlans)
            .WithOne(p => p.Line)
            .HasForeignKey(p => p.LineId)
            .HasConstraintName("fk_maintenance_plan_line")
            .OnDelete(DeleteBehavior.Restrict);
    }
}