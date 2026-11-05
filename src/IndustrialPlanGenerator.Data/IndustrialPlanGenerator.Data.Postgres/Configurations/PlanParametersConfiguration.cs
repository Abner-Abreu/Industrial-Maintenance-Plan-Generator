namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlanParametersConfiguration : IEntityTypeConfiguration<PlanParameters>
{
    public void Configure(EntityTypeBuilder<PlanParameters> builder)
    {
        builder.ToTable("plan_parameters", t =>
        {
            t.HasCheckConstraint("ck_plan_parameters_preventive_proportion",
                "preventive_proportion >= 0 AND preventive_proportion <= 100");
            t.HasCheckConstraint("ck_plan_parameters_corrective_proportion",
                "corrective_proportion >= 0 AND corrective_proportion <= 100");
            t.HasCheckConstraint("ck_plan_parameters_predictive_proportion",
                "predictive_proportion >= 0 AND predictive_proportion <= 100");
            t.HasCheckConstraint("ck_plan_parameters_system_coverage",
                "system_coverage >= 0 AND system_coverage <= 100");
        });

        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(pp => pp.PreventiveProportion)
            .HasColumnName("preventive_proportion")
            .IsRequired()
            .HasColumnType("numeric(5,2)");

        builder.Property(pp => pp.CorrectiveProportion)
            .HasColumnName("corrective_proportion")
            .IsRequired()
            .HasColumnType("numeric(5,2)");

        builder.Property(pp => pp.PredictiveProportion)
            .HasColumnName("predictive_proportion")
            .IsRequired()
            .HasColumnType("numeric(5,2)");

        builder.Property(pp => pp.TotalTasks)
            .HasColumnName("total_tasks")
            .IsRequired();

        builder.Property(pp => pp.SystemCoverage)
            .HasColumnName("system_coverage")
            .IsRequired()
            .HasColumnType("numeric(5,2)");

        // RI-12: FK plan_parameters.plan_id -> maintenance_plans.id (1:1)
        builder.HasOne(pp => pp.MaintenancePlan)
            .WithOne(p => p.PlanParameters)
            .HasForeignKey<PlanParameters>(pp => pp.PlanId)
            .HasConstraintName("fk_plan_parameters_plan")
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on PlanId for 1:1 relationship
        builder.HasIndex(pp => pp.PlanId)
            .IsUnique()
            .HasDatabaseName("ux_plan_parameters_plan_id");
    }
}