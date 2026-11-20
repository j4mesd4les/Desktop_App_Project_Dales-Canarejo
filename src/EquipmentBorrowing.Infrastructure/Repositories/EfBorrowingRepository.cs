using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Models;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using EquipmentBorrowing.Application.Errors;

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

    // History and search: filter and sort on the joined entities FIRST and
    // project into BorrowingHistoryRecord only at the very end. EF Core cannot
    // translate Where/OrderBy applied after a constructor projection.
    public async Task<IReadOnlyList<BorrowingHistoryRecord>> GetHistoryAsync(
        int? studentId = null, int? equipmentId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = from b in context.Borrowings.AsNoTracking()
                    join s in context.Students on b.StudentId equals s.Id
                    join e in context.Equipment on b.EquipmentId equals e.Id
                    select new { b, s, e };

        if (studentId.HasValue) query = query.Where(x => x.b.StudentId == studentId.Value);
        if (equipmentId.HasValue) query = query.Where(x => x.b.EquipmentId == equipmentId.Value);

        return await query
            .OrderByDescending(x => x.b.BorrowedOn)
            .ThenByDescending(x => x.b.Id)
            .Select(x => new BorrowingHistoryRecord(x.b.Id, x.b.StudentId, x.s.Name, x.b.EquipmentId,
                x.e.Name, x.b.BorrowedOn, x.b.ExpectedReturnOn, x.b.ReturnedOn, x.b.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BorrowingHistoryRecord>> SearchAsync(string? text,
        BorrowingStatusFilter status, BorrowingSortField sortField, bool descending,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = from b in context.Borrowings.AsNoTracking()
                    join s in context.Students on b.StudentId equals s.Id
                    join e in context.Equipment on b.EquipmentId equals e.Id
                    select new { b, s, e };

        var term = text?.Trim();
        if (!string.IsNullOrEmpty(term))
        {
            // % and _ typed by the user are matched literally, not as wildcards.
            var pattern = $"%{EscapeLike(term)}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.s.Name, pattern, "\\") ||
                EF.Functions.ILike(x.s.StudentNumber, pattern, "\\") ||
                EF.Functions.ILike(x.e.Name, pattern, "\\"));
        }

        if (status == BorrowingStatusFilter.Active)
            query = query.Where(x => x.b.Status == BorrowingStatus.Active);
        else if (status == BorrowingStatusFilter.Returned)
            query = query.Where(x => x.b.Status == BorrowingStatus.Returned);

        // The borrowing id is always the tie-breaker so the order is stable.
        var ordered = sortField == BorrowingSortField.CheckoutDate
            ? (descending
                ? query.OrderByDescending(x => x.b.BorrowedOn).ThenByDescending(x => x.b.Id)
                : query.OrderBy(x => x.b.BorrowedOn).ThenBy(x => x.b.Id))
            : (descending
                ? query.OrderByDescending(x => x.b.ExpectedReturnOn).ThenByDescending(x => x.b.Id)
                : query.OrderBy(x => x.b.ExpectedReturnOn).ThenBy(x => x.b.Id));

        return await ordered
            .Select(x => new BorrowingHistoryRecord(x.b.Id, x.b.StudentId, x.s.Name, x.b.EquipmentId,
                x.e.Name, x.b.BorrowedOn, x.b.ExpectedReturnOn, x.b.ReturnedOn, x.b.Status))
            .ToListAsync(cancellationToken);
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    public async Task<IReadOnlyList<Borrowing>> CheckoutAtomicallyAsync(int studentId,
        IReadOnlyList<int> equipmentIds, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        if (equipmentIds.Count == 0 || equipmentIds.Distinct().Count() != equipmentIds.Count)
            throw new ArgumentException("Select one or more distinct equipment items.");
        if (dueDate.Date <= DateTime.Today) throw new ArgumentException("Expected return date must be after today.");
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var student = await context.Students.FromSqlInterpolated($"SELECT * FROM \"Students\" WHERE \"Id\" = {studentId} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
            if (student is null) throw new StudentNotFoundError("The selected student no longer exists.");
            if (!student.IsAllowedToBorrow) throw new StudentNotAllowedToBorrowError("This student is not eligible to borrow.");
            var activeCount = await context.Borrowings.CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);
            if (activeCount + equipmentIds.Count > 3) throw new BorrowingLimitExceededError("This checkout would exceed the limit of three active items.");
            var ids = equipmentIds.OrderBy(id => id).ToArray();
            var items = await context.Equipment.FromSqlInterpolated($"SELECT * FROM \"Equipment\" WHERE \"Id\" = ANY ({ids}) ORDER BY \"Id\" FOR UPDATE").ToListAsync(cancellationToken);
            if (items.Count != ids.Length) throw new EquipmentNotFoundError("One or more selected items no longer exist.");
            var activeItems = await context.Borrowings.Where(b => ids.Contains(b.EquipmentId) && b.Status == BorrowingStatus.Active).Select(b => b.EquipmentId).ToListAsync(cancellationToken);
            if (items.Any(i => !i.IsAvailable) || activeItems.Count > 0) throw new EquipmentNotAvailableError("One or more selected items are no longer available. Refresh and try again.");
            foreach (var item in items) item.IsAvailable = false;
            var records = items.Select(item => new Borrowing { StudentId = studentId, EquipmentId = item.Id, BorrowedOn = DateTime.Today, ExpectedReturnOn = dueDate.Date }).ToList();
            context.Borrowings.AddRange(records);
            await context.SaveChangesAsync(cancellationToken);
            try { await transaction.CommitAsync(cancellationToken); }
            catch (Exception ex) { throw new CommitOutcomeUnknownError("The checkout commit result is unknown. Refresh borrowing and equipment records before deciding whether to retry.", ex); }
            return records;
        }
        catch (CommitOutcomeUnknownError) { throw; }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }

    public async Task<Borrowing> ReturnAtomicallyAsync(int borrowingId, DateTime returnedOn,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var borrowing = await context.Borrowings.FirstOrDefaultAsync(b => b.Id == borrowingId, cancellationToken)
                ?? throw new BorrowingNotFoundError($"Borrowing {borrowingId} does not exist.");
            if (borrowing.Status != BorrowingStatus.Active) throw new BorrowingAlreadyReturnedError("This borrowing was already returned.");
            if (returnedOn.Date < borrowing.BorrowedOn.Date) throw new InvalidOperationException("Return date cannot be before checkout.");
            var item = await context.Equipment.FromSqlInterpolated($"SELECT * FROM \"Equipment\" WHERE \"Id\" = {borrowing.EquipmentId} FOR UPDATE").SingleAsync(cancellationToken);
            borrowing.MarkReturned(returnedOn.Date);
            item.IsAvailable = true;
            await context.SaveChangesAsync(cancellationToken);
            try { await transaction.CommitAsync(cancellationToken); }
            catch (Exception ex) { throw new CommitOutcomeUnknownError("The return commit result is unknown. Refresh borrowing and equipment records before deciding whether to retry.", ex); }
            return borrowing;
        }
        catch (CommitOutcomeUnknownError) { throw; }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }
}