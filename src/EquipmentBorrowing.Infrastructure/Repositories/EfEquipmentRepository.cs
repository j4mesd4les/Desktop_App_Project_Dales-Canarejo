using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Database-backed equipment repository (replaces InMemoryEquipmentRepository).
/// </summary>
public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _contextFactory;

    public EfEquipmentRepository(
        IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Display-only list: no tracking.
        return await context.Equipment
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    // LINQ query 1: equipment that can be borrowed right now.
    // Generated SQL is roughly:
    //   SELECT ... FROM "Equipment" AS "e" WHERE "e"."IsAvailable" ORDER BY "e"."Id"
    public async Task<IReadOnlyList<Equipment>> GetAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Equipment
            .AsNoTracking()
            .Where(e => e.IsAvailable)
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Equipment?> GetByIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        // The service changes this object (IsAvailable) and hands it back to
        // SaveAsync, where it is written to the database. The context that
        // loads it is disposed right here, so tracking it would do nothing.
        // The tracked update happens inside SaveAsync.
        return await context.Equipment
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == equipmentId, cancellationToken);
    }

    public async Task SaveAsync(
        Equipment equipment,
        CancellationToken cancellationToken = default)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Tracking IS wanted here: we load the stored row with tracking on,
        // copy the new values onto it, and EF compares old and new to work
        // out what changed. SaveChanges then sends an UPDATE for only the
        // changed columns (for a borrow, just IsAvailable).
        var existing = await context.Equipment
            .FirstOrDefaultAsync(e => e.Id == equipment.Id, cancellationToken);

        if (existing is null)
        {
            // Same behaviour as the in-memory version, which added the item
            // if it was not already stored.
            context.Equipment.Add(equipment);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(equipment);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
