namespace IndustrialPlanGenerator.Data.Postgres.Configurations;

using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensors", t =>
        {
            t.HasCheckConstraint("ck_sensor_type",
                $"type IN ('{SensorType.Temperature}', '{SensorType.Vibration}', '{SensorType.Pressure}')");
        });

        builder.HasKey(s => s.Id);

        builder.Property(s => s.EquipmentId)
            .HasColumnName("equipment_id")
            .IsRequired();

        builder.Property(s => s.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>();

        // RI-07: FK sensors.equipment_id -> equipment_systems.id
        builder.HasOne(s => s.Equipment)
            .WithMany(e => e.Sensors)
            .HasForeignKey(s => s.EquipmentId)
            .HasConstraintName("fk_sensor_equipment")
            .OnDelete(DeleteBehavior.Cascade);

        // Index for FK
        builder.HasIndex(s => s.EquipmentId)
            .HasDatabaseName("ix_sensor_equipment_id");
    }
}