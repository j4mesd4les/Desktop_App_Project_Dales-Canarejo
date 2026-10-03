using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings", table =>
        {
            // A borrowing must be due after the day it was borrowed.
            table.HasCheckConstraint(
                "CK_Borrowings_DueAfterBorrowed",
                "ExpectedReturnOn > BorrowedOn");

            // Status and ReturnedOn must agree: an Active borrowing has no
            // return date, a Returned one always has one.
            table.HasCheckConstraint(
                "CK_Borrowings_StatusMatchesReturnedOn",
                "(Status = 'Active' AND ReturnedOn IS NULL) " +
                "OR (Status = 'Returned' AND ReturnedOn IS NOT NULL)");
        });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BorrowedOn).IsRequired();
        builder.Property(b => b.ExpectedReturnOn).IsRequired();

        // Optional: null until the equipment is returned.
        builder.Property(b => b.ReturnedOn)
            .IsRequired(false);

        // Stored as readable text ("Active" / "Returned") instead of 0 / 1,
        // so the SQL in docs/database-queries.sql is easy to read.
        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Each borrowing belongs to exactly one student and one equipment.
        // The domain classes have no navigation properties, so the
        // relationships are declared here with only the foreign keys.
        // Restrict: a student or equipment with borrowing history cannot be
        // deleted by accident.
        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Speeds up "count active borrowings for a student", which runs on
        // every borrow attempt.
        builder.HasIndex(b => new { b.StudentId, b.Status });

        // Speeds up borrowing history for one piece of equipment.
        builder.HasIndex(b => b.EquipmentId);

        builder.HasData(SeedData.Borrowings);
    }
}
