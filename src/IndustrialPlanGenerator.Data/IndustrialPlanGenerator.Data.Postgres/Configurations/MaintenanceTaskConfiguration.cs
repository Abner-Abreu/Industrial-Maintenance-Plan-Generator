namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MaintenanceTaskConfiguration : IEntityTypeConfiguration<MaintenanceTask>
{
    public void Configure(EntityTypeBuilder<MaintenanceTask> builder)
    {
        builder.ToTable("maintenance_tasks", t =>
        {
            t.HasCheckConstraint("ck_maintenance_task_type",
                $"type IN ('{MaintenanceType.Preventive}', '{MaintenanceType.Corrective}', '{MaintenanceType.Predictive}')");
            t.HasCheckConstraint("ck_maintenance_task_criticality",
                $"criticality IN ('{Criticality.Low}', '{Criticality.Medium}', '{Criticality.High}')");
            t.HasCheckConstraint("ck_maintenance_task_status",
                $"status IN ('{TaskStatus.Active}', '{TaskStatus.Duplicated}')");
        });

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.SystemId)
            .HasColumnName("system_id")
            .IsRequired();

        builder.Property(t => t.Criticality)
            .HasColumnName("criticality")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.ThechnicianCreatorId)
            .HasColumnName("technician_creator_id")
            .IsRequired();

        builder.Property(t => t.CreationDate)
            .HasColumnName("creation_date")
            .IsRequired()
            .HasColumnType("date");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>();

        // RI-08: FK maintenance_tasks.system_id -> equipment_systems.id
        builder.HasOne(t => t.EquipmentSystem)
            .WithMany(e => e.MaintenanceTasks)
            .HasForeignKey(t => t.SystemId)
            .HasConstraintName("fk_maintenance_task_equipment")
            .OnDelete(DeleteBehavior.Restrict);

        // RI-09: FK maintenance_tasks.technician_creator_id -> technicians.id
        builder.HasOne(t => t.Technician)
            .WithMany(t => t.MaintenanceTasks)
            .HasForeignKey(t => t.ThechnicianCreatorId)
            .HasConstraintName("fk_maintenance_task_technician")
            .OnDelete(DeleteBehavior.Restrict);

        // RI-29: Unique constraint for active tasks (description, system_id, type, criticality)
        // Implemented as partial unique index
        builder.HasIndex(t => new { t.Description, t.SystemId, t.Type, t.Criticality })
            .IsUnique()
            .HasFilter("status = 'Active'")
            .HasDatabaseName("ux_maintenance_task_active_unique");

        // Indexes for FKs
        builder.HasIndex(t => t.SystemId)
            .HasDatabaseName("ix_maintenance_task_system_id");

        builder.HasIndex(t => t.ThechnicianCreatorId)
            .HasDatabaseName("ix_maintenance_task_technician_creator_id");

        // Relationships
        builder.HasMany(t => t.PlanTasks)
            .WithOne(pt => pt.MaintenanceTask)
            .HasForeignKey(pt => pt.TaskId)
            .HasConstraintName("fk_plan_task_task")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.TaskExecutions)
            .WithOne(te => te.MaintenanceTask)
            .HasForeignKey(te => te.TaskId)
            .HasConstraintName("fk_task_execution_task")
            .OnDelete(DeleteBehavior.Restrict);
    }
}