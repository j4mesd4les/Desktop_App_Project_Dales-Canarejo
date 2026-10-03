using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// The application's gateway to the SQLite database. It knows which entities
/// exist (the DbSets), how they map to tables (the configurations), and it
/// tracks changes until SaveChangesAsync writes them to the database.
/// Only the Infrastructure layer uses it; Views, ViewModels and Application
/// services never see it.
/// </summary>
public class EquipmentBorrowingDbContext : DbContext
{
    public EquipmentBorrowingDbContext(
        DbContextOptions<EquipmentBorrowingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> in this assembly
        // (StudentConfiguration, EquipmentConfiguration, BorrowingConfiguration),
        // so the mapping rules live outside the domain classes.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(EquipmentBorrowingDbContext).Assembly);
    }
}
