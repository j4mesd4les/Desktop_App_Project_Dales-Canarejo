using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        // Each physical item has its own label ("Laptop 01", "Laptop 02"),
        // so the name is unique.
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(50);

        // Not unique: used for filtering and grouping equipment by type.
        builder.HasIndex(e => e.Type);

        builder.Property(e => e.IsAvailable)
            .IsRequired();

        // AvailabilityStatus is computed from IsAvailable for display only.
        // It is not stored, so it must not become a column.
        builder.Ignore(e => e.AvailabilityStatus);

        builder.HasData(SeedData.Equipment);
    }
}
