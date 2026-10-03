using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// Initial data inserted by the InitialCreate migration (EF Core "HasData").
/// Because it lives in the migration, it is inserted exactly once when the
/// database is created and is never re-applied on later app starts.
///
/// IDs are fixed on purpose: HasData needs known keys, and dates must be
/// constants (not DateTime.Today) so the model snapshot stays stable.
///
/// The data covers the scenarios the lab asks for:
///   - available equipment (1-4)
///   - borrowed / unavailable equipment (5-8)
///   - several students, one of whom may not borrow (Carla)
///   - a student at the borrowing limit of 3 (Ben)
///   - a returned borrowing, so there is history
/// </summary>
internal static class SeedData
{
    public static readonly Student[] Students =
    [
        new() { Id = 1, StudentNumber = "2023-0001", Name = "Dodong", IsAllowedToBorrow = true },
        new() { Id = 2, StudentNumber = "2023-0002", Name = "Ana Reyes", IsAllowedToBorrow = true },
        new() { Id = 3, StudentNumber = "2023-0003", Name = "Ben Santos", IsAllowedToBorrow = true },
        new() { Id = 4, StudentNumber = "2023-0004", Name = "Carla Mendoza", IsAllowedToBorrow = false },
    ];

    public static readonly Equipment[] Equipment =
    [
        new() { Id = 1, Name = "HDMI Projector", Type = "Audio Visual", IsAvailable = true },
        new() { Id = 2, Name = "Laptop", Type = "Computer", IsAvailable = true },
        new() { Id = 3, Name = "Extension Cord", Type = "Accessory", IsAvailable = true },
        new() { Id = 4, Name = "Speaker", Type = "Audio Visual", IsAvailable = true },
        new() { Id = 5, Name = "Laptop 02", Type = "Computer", IsAvailable = false },
        new() { Id = 6, Name = "Camera", Type = "Photography", IsAvailable = false },
        new() { Id = 7, Name = "Tripod", Type = "Photography", IsAvailable = false },
        new() { Id = 8, Name = "Microphone", Type = "Audio Visual", IsAvailable = false },
    ];

    public static readonly Borrowing[] Borrowings =
    [
        // Active: Ana has Laptop 02.
        new()
        {
            Id = 1, StudentId = 2, EquipmentId = 5,
            BorrowedOn = new DateTime(2026, 10, 1), ExpectedReturnOn = new DateTime(2026, 10, 8),
            Status = BorrowingStatus.Active, ReturnedOn = null
        },
        // Active: Ben has three items, so he is at the limit.
        new()
        {
            Id = 2, StudentId = 3, EquipmentId = 6,
            BorrowedOn = new DateTime(2026, 10, 1), ExpectedReturnOn = new DateTime(2026, 10, 8),
            Status = BorrowingStatus.Active, ReturnedOn = null
        },
        new()
        {
            Id = 3, StudentId = 3, EquipmentId = 7,
            BorrowedOn = new DateTime(2026, 10, 1), ExpectedReturnOn = new DateTime(2026, 10, 8),
            Status = BorrowingStatus.Active, ReturnedOn = null
        },
        new()
        {
            Id = 4, StudentId = 3, EquipmentId = 8,
            BorrowedOn = new DateTime(2026, 10, 1), ExpectedReturnOn = new DateTime(2026, 10, 8),
            Status = BorrowingStatus.Active, ReturnedOn = null
        },
        // Returned: Dodong borrowed the Speaker earlier and brought it back.
        new()
        {
            Id = 5, StudentId = 1, EquipmentId = 4,
            BorrowedOn = new DateTime(2026, 9, 20), ExpectedReturnOn = new DateTime(2026, 9, 27),
            Status = BorrowingStatus.Returned, ReturnedOn = new DateTime(2026, 9, 25)
        },
    ];
}
