namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OperatorLineConfiguration : IEntityTypeConfiguration<OperatorLine>
{
    public void Configure(EntityTypeBuilder<OperatorLine> builder)
    {
        builder.ToTable("operator_lines");

        // RI-03: Composite primary key (operator_id, line_id)
        builder.HasKey(ol => new { ol.OperatorId, ol.LineId });

        builder.Property(ol => ol.OperatorId)
            .HasColumnName("operator_id")
            .IsRequired();

        builder.Property(ol => ol.LineId)
            .HasColumnName("line_id")
            .IsRequired();

        // RI-19: FK operator_lines.operator_id -> operators.id
        builder.HasOne(ol => ol.Operator)
            .WithMany(o => o.Lines)
            .HasForeignKey(ol => ol.OperatorId)
            .HasConstraintName("fk_operator_line_operator")
            .OnDelete(DeleteBehavior.Cascade);

        // RI-19: FK operator_lines.line_id -> production_lines.id
        builder.HasOne(ol => ol.Line)
            .WithMany(l => l.Operators)
            .HasForeignKey(ol => ol.LineId)
            .HasConstraintName("fk_operator_line_line")
            .OnDelete(DeleteBehavior.Cascade);

        // Index for FK on LineId
        builder.HasIndex(ol => ol.LineId)
            .HasDatabaseName("ix_operator_line_line_id");
    }
}