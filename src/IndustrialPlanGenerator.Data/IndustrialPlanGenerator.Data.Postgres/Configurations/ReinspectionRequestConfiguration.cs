namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ReinspectionRequestConfiguration : IEntityTypeConfiguration<ReinspectionRequest>
{
    public void Configure(EntityTypeBuilder<ReinspectionRequest> builder)
    {
        builder.ToTable("reinspection_requests", t =>
        {
            t.HasCheckConstraint("ck_reinspection_request_status",
                $"status IN ('{ReinspectionStatus.Pending}', '{ReinspectionStatus.InProgress}', '{ReinspectionStatus.Completed}')");
        });

        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.ExecutionId)
            .HasColumnName("execution_id")
            .IsRequired();

        builder.Property(rr => rr.OperatorId)
            .HasColumnName("operator_id")
            .IsRequired();

        builder.Property(rr => rr.SupportTechnicianId)
            .HasColumnName("support_technician_id");

        builder.Property(rr => rr.RequestDate)
            .HasColumnName("request_date")
            .IsRequired()
            .HasColumnType("date");

        builder.Property(rr => rr.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(rr => rr.ManualResult)
            .HasColumnName("manual_result")
            .HasMaxLength(2000);

        builder.Property(rr => rr.ResultDate)
            .HasColumnName("result_date")
            .HasColumnType("date");

        // RI-17: FK reinspection_requests.execution_id -> task_executions.id
        builder.HasOne(rr => rr.TaskExecution)
            .WithMany(te => te.ReinspectionRequests)
            .HasForeignKey(rr => rr.ExecutionId)
            .HasConstraintName("fk_reinspection_request_execution")
            .OnDelete(DeleteBehavior.Cascade);

        // RI-17: FK reinspection_requests.operator_id -> operators.id
        builder.HasOne(rr => rr.Operator)
            .WithMany(o => o.ReinspectionRequests)
            .HasForeignKey(rr => rr.OperatorId)
            .HasConstraintName("fk_reinspection_request_operator")
            .OnDelete(DeleteBehavior.Restrict);

        // RI-17: FK reinspection_requests.support_technician_id -> technicians.id (nullable)
        builder.HasOne(rr => rr.Technician)
            .WithMany(t => t.ReinspectionRequests)
            .HasForeignKey(rr => rr.SupportTechnicianId)
            .HasConstraintName("fk_reinspection_request_support_technician")
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for FKs
        builder.HasIndex(rr => rr.ExecutionId)
            .HasDatabaseName("ix_reinspection_request_execution_id");

        builder.HasIndex(rr => rr.OperatorId)
            .HasDatabaseName("ix_reinspection_request_operator_id");

        builder.HasIndex(rr => rr.SupportTechnicianId)
            .HasDatabaseName("ix_reinspection_request_support_technician_id");
    }
}