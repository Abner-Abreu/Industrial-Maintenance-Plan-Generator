namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlanAssignmentConfiguration : IEntityTypeConfiguration<PlanAssignment>
{
    public void Configure(EntityTypeBuilder<PlanAssignment> builder)
    {
        builder.ToTable("plan_assignments");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(pa => pa.OperatorId)
            .HasColumnName("operator_id")
            .IsRequired();

        // RI-15: FK plan_assignments.plan_id -> maintenance_plans.id
        builder.HasOne(pa => pa.MaintenancePlan)
            .WithMany(p => p.PlanAssignments)
            .HasForeignKey(pa => pa.PlanId)
            .HasConstraintName("fk_plan_assignment_plan")
            .OnDelete(DeleteBehavior.Cascade);

        // RI-15: FK plan_assignments.operator_id -> operators.id
        builder.HasOne(pa => pa.Operator)
            .WithMany(o => o.Plans)
            .HasForeignKey(pa => pa.OperatorId)
            .HasConstraintName("fk_plan_assignment_operator")
            .OnDelete(DeleteBehavior.Restrict);

        // RI-35: Unique constraint (plan_id, operator_id) - no double assignment
        builder.HasIndex(pa => new { pa.PlanId, pa.OperatorId })
            .IsUnique()
            .HasDatabaseName("ux_plan_assignment_operator");

        // Indexes for FKs
        builder.HasIndex(pa => pa.PlanId)
            .HasDatabaseName("ix_plan_assignment_plan_id");

        builder.HasIndex(pa => pa.OperatorId)
            .HasDatabaseName("ix_plan_assignment_operator_id");

        // 1:N with TaskExecution
        builder.HasMany(pa => pa.TaskExecutions)
            .WithOne(te => te.PlanAssignment)
            .HasForeignKey(te => te.AssignmentId)
            .HasConstraintName("fk_task_execution_assignment")
            .OnDelete(DeleteBehavior.Cascade);
    }
}