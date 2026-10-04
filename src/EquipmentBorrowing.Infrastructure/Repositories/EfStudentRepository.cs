using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Database-backed student repository. It implements the same
/// IStudentRepository interface as the in-memory version, so the application
/// services do not know (or care) that SQLite is now behind it.
///
/// Each method creates its own short-lived DbContext from the factory and
/// disposes it when done. A desktop app keeps its ViewModels alive for the
/// whole session, so a single long-lived DbContext would slowly go stale.
/// </summary>
public class EfStudentRepository : IStudentRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _contextFactory;

    public EfStudentRepository(
        IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        // AsNoTracking: the borrow service only reads the student (name,
        // IsAllowedToBorrow) and never changes it, so EF does not need to
        // watch this object for changes.
        return await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Display-only list for the student dropdown, so no tracking.
        return await context.Students
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }
}
