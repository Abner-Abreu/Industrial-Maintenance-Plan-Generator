namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TaskExecutionConfiguration : IEntityTypeConfiguration<TaskExecution>
{
    public void Configure(EntityTypeBuilder<TaskExecution> builder)
    {
        builder.ToTable("task_executions");

        builder.HasKey(te => te.Id);

        builder.Property(te => te.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(te => te.AssignmentId)
            .HasColumnName("assignment_id")
            .IsRequired();

        builder.Property(te => te.Date)
            .HasColumnName("date")
            .IsRequired()
            .HasColumnType("date");

        builder.Property(te => te.Observations)
            .HasColumnName("observations")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(te => te.Completed)
            .HasColumnName("completed")
            .IsRequired()
            .HasDefaultValue(false);

        // RI-16: FK task_executions.assignment_id -> plan_assignments.id
        builder.HasOne(te => te.PlanAssignment)
            .WithMany(pa => pa.TaskExecutions)
            .HasForeignKey(te => te.AssignmentId)
            .HasConstraintName("fk_task_execution_assignment")
            .OnDelete(DeleteBehavior.Cascade);

        // RI-16: FK task_executions.task_id -> maintenance_tasks.id
        builder.HasOne(te => te.MaintenanceTask)
            .WithMany(t => t.TaskExecutions)
            .HasForeignKey(te => te.TaskId)
            .HasConstraintName("fk_task_execution_task")
            .OnDelete(DeleteBehavior.Restrict);

        // 1:N with ReinspectionRequest
        builder.HasMany(te => te.ReinspectionRequests)
            .WithOne(rr => rr.TaskExecution)
            .HasForeignKey(rr => rr.ExecutionId)
            .HasConstraintName("fk_reinspection_request_execution")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for FKs
        builder.HasIndex(te => te.TaskId)
            .HasDatabaseName("ix_task_execution_task_id");

        builder.HasIndex(te => te.AssignmentId)
            .HasDatabaseName("ix_task_execution_assignment_id");
    }
}