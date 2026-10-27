namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MaintenancePlanConfiguration : IEntityTypeConfiguration<MaintenancePlan>
{
    public void Configure(EntityTypeBuilder<MaintenancePlan> builder)
    {
        builder.ToTable("maintenance_plans", t =>
        {
            t.HasCheckConstraint("ck_maintenance_plan_status",
                $"status IN ('{PlanStatus.Generated}', '{PlanStatus.Validated}', '{PlanStatus.Rejected}')");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.LineId)
            .HasColumnName("line_id")
            .IsRequired();

        builder.Property(p => p.TechnicianCreatorId)
            .HasColumnName("technician_creator_id")
            .IsRequired();

        builder.Property(p => p.CreationDate)
            .HasColumnName("creation_date")
            .IsRequired()
            .HasColumnType("date");

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.AllCriteriaMet)
            .HasColumnName("all_criteria_met")
            .IsRequired()
            .HasDefaultValue(false);

        // RI-10: FK maintenance_plans.line_id -> production_lines.id
        builder.HasOne(p => p.Line)
            .WithMany(l => l.MaintenancePlans)
            .HasForeignKey(p => p.LineId)
            .HasConstraintName("fk_maintenance_plan_line")
            .OnDelete(DeleteBehavior.Restrict);

        // RI-11: FK maintenance_plans.technician_creator_id -> technicians.id
        builder.HasOne(p => p.Technician)
            .WithMany(t => t.MaintenancePlans)
            .HasForeignKey(p => p.TechnicianCreatorId)
            .HasConstraintName("fk_maintenance_plan_technician")
            .OnDelete(DeleteBehavior.Restrict);

        // 1:1 relationship with PlanParameters (RI-12)
        builder.HasOne(p => p.PlanParameters)
            .WithOne(pp => pp.MaintenancePlan)
            .HasForeignKey<PlanParameters>(pp => pp.PlanId)
            .HasConstraintName("fk_plan_parameters_plan")
            .OnDelete(DeleteBehavior.Cascade);

        // N:M with MaintenanceTask through PlanTask (RI-13)
        builder.HasMany(p => p.PlanTasks)
            .WithOne(pt => pt.MaintenancePlan)
            .HasForeignKey(pt => pt.PlanId)
            .HasConstraintName("fk_plan_task_plan")
            .OnDelete(DeleteBehavior.Cascade);

        // 1:N with PlanValidation
        builder.HasMany(p => p.PlanValidations)
            .WithOne(pv => pv.MaintenancePlan)
            .HasForeignKey(pv => pv.MaintancePlan)
            .HasConstraintName("fk_plan_validation_plan")
            .OnDelete(DeleteBehavior.Cascade);

        // 1:N with PlanAssignment
        builder.HasMany(p => p.PlanAssignments)
            .WithOne(pa => pa.MaintenancePlan)
            .HasForeignKey(pa => pa.PlanId)
            .HasConstraintName("fk_plan_assignment_plan")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for FKs
        builder.HasIndex(p => p.LineId)
            .HasDatabaseName("ix_maintenance_plan_line_id");

        builder.HasIndex(p => p.TechnicianCreatorId)
            .HasDatabaseName("ix_maintenance_plan_technician_creator_id");
    }
}