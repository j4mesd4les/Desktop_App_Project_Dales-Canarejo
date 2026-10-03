using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Models;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Database-backed borrowing repository (replaces InMemoryBorrowingRepository).
/// </summary>
public class EfBorrowingRepository : IBorrowingRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _contextFactory;

    public EfBorrowingRepository(
        IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        // The service creates the borrowing with Id = 0. The database assigns
        // the real id on insert, and EF copies it back onto this object, so
        // borrowing.Id is correct after SaveChangesAsync (same as the
        // in-memory version, which assigned _nextId++).
        context.Borrowings.Add(borrowing);
        await context.SaveChangesAsync(cancellationToken);
    }

    // LINQ query 3: how many borrowings a student currently has open.
    // The borrow service uses this to enforce the limit of 3.
    // Generated SQL is roughly:
    //   SELECT COUNT(*) FROM "Borrowings" AS "b"
    //   WHERE "b"."StudentId" = @studentId AND "b"."Status" = 'Active'
    public async Task<int> CountActiveForStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Borrowings.CountAsync(
            b => b.StudentId == studentId &&
                 b.Status == BorrowingStatus.Active,
            cancellationToken);
    }

    public async Task<Borrowing?> GetByIdAsync(
        int borrowingId,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Loaded without tracking; the return service changes it and passes
        // it back to SaveAsync (see EfEquipmentRepository.GetByIdAsync).
        return await context.Borrowings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == borrowingId, cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Borrowings
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Tracking IS wanted: load the stored row, copy the new values on,
        // and let EF detect the change. For a return, that is an UPDATE of
        // Status and ReturnedOn only.
        var existing = await context.Borrowings
            .FirstOrDefaultAsync(b => b.Id == borrowing.Id, cancellationToken);

        if (existing is null)
        {
            context.Borrowings.Add(borrowing);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(borrowing);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // LINQ query 2: active borrowings together with the student and
    // equipment names. Borrowing only stores ids, so this joins three tables
    // in a single SQL query instead of asking the database once per row.
    // Generated SQL is roughly:
    //   SELECT "b"."Id", "s"."Name", "e"."Name", "b"."BorrowedOn", "b"."ExpectedReturnOn"
    //   FROM "Borrowings" AS "b"
    //   INNER JOIN "Students"  AS "s" ON "b"."StudentId"   = "s"."Id"
    //   INNER JOIN "Equipment" AS "e" ON "b"."EquipmentId" = "e"."Id"
    //   WHERE "b"."Status" = 'Active'
    //   ORDER BY "b"."ExpectedReturnOn", "b"."Id"
    public async Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query =
            from b in context.Borrowings
            join s in context.Students on b.StudentId equals s.Id
            join e in context.Equipment on b.EquipmentId equals e.Id
            where b.Status == BorrowingStatus.Active
            orderby b.ExpectedReturnOn, b.Id
            select new ActiveBorrowingDetails(
                b.Id,
                s.Name,
                e.Name,
                b.BorrowedOn,
                b.ExpectedReturnOn);

        // Display-only. (A projection into a non-entity type is not tracked
        // anyway; AsNoTracking just states the intent.)
        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }
}
