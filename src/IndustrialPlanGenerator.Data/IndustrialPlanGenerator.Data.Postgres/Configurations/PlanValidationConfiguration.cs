namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlanValidationConfiguration : IEntityTypeConfiguration<PlanValidation>
{
    public void Configure(EntityTypeBuilder<PlanValidation> builder)
    {
        builder.ToTable("plan_validations", t =>
        {
            t.HasCheckConstraint("ck_plan_validation_result",
                $"result IN ('{ValidationResult.Approved}', '{ValidationResult.Rejected}')");
        });

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.MaintenancePlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(pv => pv.PlantManagerId)
            .HasColumnName("plant_manager_id")
            .IsRequired();

        builder.Property(pv => pv.ValidationDate)
            .HasColumnName("validation_date")
            .IsRequired()
            .HasColumnType("date");

        builder.Property(pv => pv.Observations)
            .HasColumnName("observations")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(pv => pv.Result)
            .HasColumnName("result")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(pv => pv.NewPlanRequired)
            .HasColumnName("new_plan_required")
            .IsRequired()
            .HasDefaultValue(false);

        // RI-14: FK plan_validations.plan_id -> maintenance_plans.id
        builder.HasOne(pv => pv.MaintenancePlan)
            .WithMany(p => p.PlanValidations)
            .HasForeignKey(pv => pv.MaintenancePlanId)
            .HasConstraintName("fk_plan_validation_plan")
            .OnDelete(DeleteBehavior.Cascade);

        // RI-14: FK plan_validations.plant_manager_id -> plant_managers.id
        builder.HasOne(pv => pv.PlantManager)
            .WithMany(jp => jp.PlanValidations)
            .HasForeignKey(pv => pv.PlantManagerId)
            .HasConstraintName("fk_plan_validation_plant_manager")
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for FKs
        builder.HasIndex(pv => pv.MaintenancePlanId)
            .HasDatabaseName("ix_plan_validation_plan_id");

        builder.HasIndex(pv => pv.PlantManagerId)
            .HasDatabaseName("ix_plan_validation_plant_manager_id");
    }
}