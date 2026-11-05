namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TechnicianLineConfiguration : IEntityTypeConfiguration<TechnicianLine>
{
    public void Configure(EntityTypeBuilder<TechnicianLine> builder)
    {
        builder.ToTable("technician_lines", t =>
        {
            t.HasCheckConstraint("ck_technician_line_permission",
                $"permission IN ('{TechnicianPermission.Generate}', '{TechnicianPermission.Validate}')");
        });

        // RI-02: Composite primary key (technician_id, line_id)
        builder.HasKey(tl => new { tl.TechnicianId, tl.LineId });

        builder.Property(tl => tl.TechnicianId)
            .HasColumnName("technician_id")
            .IsRequired();

        builder.Property(tl => tl.LineId)
            .HasColumnName("line_id")
            .IsRequired();

        builder.Property(tl => tl.Permission)
            .HasColumnName("permission")
            .IsRequired()
            .HasConversion<string>();

        // RI-18: FK technician_lines.technician_id -> technicians.id
        builder.HasOne(tl => tl.Technician)
            .WithMany(t => t.Lines)
            .HasForeignKey(tl => tl.TechnicianId)
            .HasConstraintName("fk_technician_line_technician")
            .OnDelete(DeleteBehavior.Cascade);

        // RI-18: FK technician_lines.line_id -> production_lines.id
        builder.HasOne(tl => tl.Line)
            .WithMany(l => l.TechnicianLines)
            .HasForeignKey(tl => tl.LineId)
            .HasConstraintName("fk_technician_line_line")
            .OnDelete(DeleteBehavior.Cascade);

        // Index for FK on LineId
        builder.HasIndex(tl => tl.LineId)
            .HasDatabaseName("ix_technician_line_line_id");
    }
}