using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// Used only by the "dotnet ef" command-line tools. The tools need to create a
/// DbContext without running the Avalonia app, so this tells them how.
/// The running application does not use this class.
/// </summary>
public class EquipmentBorrowingDbContextFactory
    : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
            .UseSqlite(DatabaseLocation.ConnectionString)
            .Options;

        return new EquipmentBorrowingDbContext(options);
    }
}
