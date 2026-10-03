using EquipmentBorrowing.Application.Models;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    // New in Lab 3: the student dropdown needs every student,
    // not just the one hard-coded student.
    Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken = default);
}

public interface IEquipmentRepository
{
    Task<IReadOnlyList<Equipment>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // New in Lab 3 (LINQ query 1): only equipment that can be borrowed now.
    Task<IReadOnlyList<Equipment>> GetAvailableAsync(
        CancellationToken cancellationToken = default);

    Task<Equipment?> GetByIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        Equipment equipment,
        CancellationToken cancellationToken = default);
}

public interface IBorrowingRepository
{
    Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveForStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<Borrowing?> GetByIdAsync(
        int borrowingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // New in Lab 3: writes changes to an existing borrowing back to storage.
    // The in-memory version did not need this because everyone shared the same
    // object; a database needs to be told to save the change (for example,
    // when a borrowing is marked Returned).
    Task SaveAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default);

    // New in Lab 3 (LINQ query 2): the active borrowings together with the
    // student and equipment names, for the Active Borrowings page.
    Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveWithDetailsAsync(
        CancellationToken cancellationToken = default);
}
