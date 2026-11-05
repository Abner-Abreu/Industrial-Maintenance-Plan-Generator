namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlanTaskConfiguration : IEntityTypeConfiguration<PlanTask>
{
    public void Configure(EntityTypeBuilder<PlanTask> builder)
    {
        builder.ToTable("plan_tasks");

        // RI-04: Composite primary key (plan_id, task_id)
        builder.HasKey(pt => new { pt.PlanId, pt.TaskId });

        builder.Property(pt => pt.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(pt => pt.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(pt => pt.Order)
            .HasColumnName("order")
            .IsRequired();

        // RI-13: FK plan_tasks.plan_id -> maintenance_plans.id
        builder.HasOne(pt => pt.MaintenancePlan)
            .WithMany(p => p.PlanTasks)
            .HasForeignKey(pt => pt.PlanId)
            .HasConstraintName("fk_plan_task_plan")
            .OnDelete(DeleteBehavior.Cascade);

        // RI-13: FK plan_tasks.task_id -> maintenance_tasks.id
        builder.HasOne(pt => pt.MaintenanceTask)
            .WithMany(t => t.PlanTasks)
            .HasForeignKey(pt => pt.TaskId)
            .HasConstraintName("fk_plan_task_task")
            .OnDelete(DeleteBehavior.Restrict);

        // Index for FK on TaskId
        builder.HasIndex(pt => pt.TaskId)
            .HasDatabaseName("ix_plan_task_task_id");
    }
}