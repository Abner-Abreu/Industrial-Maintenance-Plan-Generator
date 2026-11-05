namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> builder)
    {
        builder.ToTable("operators", t =>
        {
            t.HasCheckConstraint("ck_operator_age", "age >= 18");
        });

        builder.HasKey(o => o.Id);

        // PK is also FK to users (1:1, shared PK)
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Operator>(o => o.Id)
            .HasConstraintName("fk_operator_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.Age)
            .HasColumnName("age")
            .IsRequired();

        builder.Property(o => o.ShiftId)
            .HasColumnName("shift_id")
            .IsRequired();

        // FK to Shift
        builder.HasOne(o => o.Shift)
            .WithMany(s => s.Operators)
            .HasForeignKey(o => o.ShiftId)
            .HasConstraintName("fk_operator_shift")
            .OnDelete(DeleteBehavior.Restrict);
    }
}