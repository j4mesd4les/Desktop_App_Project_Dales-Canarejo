# Generated SQL and Tracking (Lab 3, Parts N and O)

The SQL below is **not written by hand**. It was captured from the Visual Studio
Output window (Debug) while the running application executed the LINQ queries.
`AddPersistence()` turns on EF Core command logging in Debug builds, which is
where these `Executed DbCommand` lines come from.

---

## Part N: Inspected queries

### Query 1: Available equipment

**Where it is used:** the "Show only available equipment" checkbox on the Equipment page.

**LINQ** (`EfEquipmentRepository.GetAvailableAsync`)

```csharp
return await context.Equipment
    .AsNoTracking()
    .Where(e => e.IsAvailable)
    .OrderBy(e => e.Id)
    .ToListAsync(cancellationToken);
```

**Generated SQL**

```sql
SELECT "e"."Id", "e"."IsAvailable", "e"."Name", "e"."Type"
FROM "Equipment" AS "e"
WHERE "e"."IsAvailable"
ORDER BY "e"."Id"
```

**Explanation:** `Where` became a SQL `WHERE`, and `OrderBy` became `ORDER BY`. The
filtering happens inside SQLite, so only the matching rows travel back to the
application. `IsAvailable` is a boolean column; SQLite stores it as 0/1, so
`WHERE "e"."IsAvailable"` means "the value is 1". EF selects only the four mapped
columns instead of `SELECT *`. Note that the computed `AvailabilityStatus` property
is not in the list, because it is not stored in the database.

---

### Query 2: Active borrowings with student and equipment

**Where it is used:** the Active Borrowings page. A `Borrowing` only stores
`StudentId` and `EquipmentId`, so the names must be joined in.

**LINQ** (`EfBorrowingRepository.GetActiveWithDetailsAsync`)

```csharp
var query =
    from b in context.Borrowings
    join s in context.Students on b.StudentId equals s.Id
    join e in context.Equipment on b.EquipmentId equals e.Id
    where b.Status == BorrowingStatus.Active
    orderby b.ExpectedReturnOn, b.Id
    select new ActiveBorrowingDetails(
        b.Id, s.Name, e.Name, b.BorrowedOn, b.ExpectedReturnOn);

return await query.AsNoTracking().ToListAsync(cancellationToken);
```

**Generated SQL**

```sql
SELECT "b"."Id", "s"."Name", "e"."Name", "b"."BorrowedOn", "b"."ExpectedReturnOn"
FROM "Borrowings" AS "b"
INNER JOIN "Students" AS "s" ON "b"."StudentId" = "s"."Id"
INNER JOIN "Equipment" AS "e" ON "b"."EquipmentId" = "e"."Id"
WHERE "b"."Status" = 'Active'
ORDER BY "b"."ExpectedReturnOn", "b"."Id"
```

**Explanation:** the two LINQ `join` clauses became two SQL `INNER JOIN`s that follow
the foreign keys, so the three tables are combined in **one** database round trip.
`BorrowingStatus.Active` (a C# enum) became the text `'Active'` because of the
enum-to-string conversion in `BorrowingConfiguration`. The `select new ...`
projection means EF asks only for the five columns the page displays. The same join
is written by hand in `docs/database-queries.sql`; LINQ did not remove the SQL
concept, EF Core generated it.

---

### Query 3: Active borrowing count for a student (borrow limit)

**Where it is used:** `BorrowEquipmentService` calls it on every borrow to enforce
the limit of 3 active borrowings.

**LINQ** (`EfBorrowingRepository.CountActiveForStudentAsync`)

```csharp
return await context.Borrowings.CountAsync(
    b => b.StudentId == studentId &&
         b.Status == BorrowingStatus.Active,
    cancellationToken);
```

**Generated SQL**

```sql
-- Parameters: @studentId = (the selected student's Id)
SELECT COUNT(*)
FROM "Borrowings" AS "b"
WHERE "b"."StudentId" = @studentId AND "b"."Status" = 'Active'
```

**Explanation:** `CountAsync` with a condition becomes `SELECT COUNT(*) ... WHERE`.
The database counts the rows and returns a single number, instead of loading every
borrowing into memory and counting in C#. The student id is sent as a **parameter**
(`@studentId`), not pasted into the SQL text, which keeps the query reusable and
prevents SQL injection. The `IX_Borrowings_StudentId_Status` index was created for
exactly this `WHERE`.

---

## Part O: Tracking and no-tracking

EF Core's change tracker remembers the entities a `DbContext` loaded so it can work
out what changed when `SaveChangesAsync` runs. That bookkeeping costs memory and time,
so it should only be on when something will be modified.

| Operation | Tracking | Why |
|---|---|---|
| Equipment list, available-only list | **No** (`AsNoTracking`) | Display only. Nothing is saved from these objects. |
| Student dropdown | **No** | Display only. |
| Active Borrowings page (join projection) | **No** | Display only; also a projection into a non-entity type. |
| Borrow: reading the student and equipment | **No** | The service only reads the student. For the equipment it reads the current state, then hands the changed object to `SaveAsync`. |
| `EfEquipmentRepository.SaveAsync` | **Yes** | The row is loaded with tracking, new values are copied onto it, and EF works out what changed. |
| `EfBorrowingRepository.SaveAsync` | **Yes** | Same idea, used when a borrowing is returned. |

### Evidence from the log

Borrowing the Laptop produced these write commands:

```sql
UPDATE "Equipment" SET "IsAvailable" = @p0
WHERE "Id" = @p1
RETURNING 1;

INSERT INTO "Borrowings" ("BorrowedOn", "EquipmentId", "ExpectedReturnOn", "ReturnedOn", "Status", "StudentId")
VALUES (@p0, @p1, @p2, @p3, @p4, @p5)
RETURNING "Id";
```

Returning it produced:

```sql
UPDATE "Equipment" SET "IsAvailable" = @p0
WHERE "Id" = @p1
RETURNING 1;

UPDATE "Borrowings" SET "ReturnedOn" = @p0, "Status" = @p1
WHERE "Id" = @p2
RETURNING 1;
```

Because the change tracker compared the stored row with the new values, EF updated
**only the columns that changed**: `IsAvailable` for equipment, and `ReturnedOn` plus
`Status` for the borrowing. The name, type and dates were not rewritten. Also,
`INSERT ... RETURNING "Id"` is how the database-generated borrowing id is copied back
onto the `Borrowing` object, so `borrowing.Id` is correct after `AddAsync`, as it was
with the in-memory repository.

The `UPDATE "Borrowings"` line also proves the Lab 3 fix to `ReturnEquipmentService`:
without `_borrowings.SaveAsync(...)`, this statement would never have run and a returned
borrowing would have come back as `Active` after a restart.

### A trade-off worth knowing

Each change costs two reads of the same row: one no-tracking read in `GetByIdAsync`
(visible as `@equipmentId` / `@borrowingId` in the log) and one tracked read inside
`SaveAsync` (visible as `@equipment_Id` / `@borrowing_Id`). This keeps the repository
interface simple and the services unchanged, because the objects are loaded in one
`DbContext`, modified in the service, and saved through another short-lived one. The
cost is one extra `SELECT` by primary key per save, which is negligible for this
application. A larger application could track the entity across the whole operation
with a scoped `DbContext` (one context per borrow or return) and save once.

Also note that the borrow writes (equipment update, then borrowing insert) run as two
separate `SaveChangesAsync` calls, so they are not one transaction. If the program
crashed between them, the equipment could be marked unavailable with no borrowing
recorded. A single scoped `DbContext` or an explicit transaction would fix that.
