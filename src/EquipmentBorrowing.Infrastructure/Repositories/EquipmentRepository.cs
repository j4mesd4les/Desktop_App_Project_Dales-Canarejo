using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Models;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

// These in-memory repositories are no longer used by the desktop app (it now
// uses the Ef* repositories). They are kept because the Tests demo project
// still runs against them, and because they show that the same interfaces can
// be satisfied by a completely different storage mechanism.

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<int, Student> _students;

    public InMemoryStudentRepository(Dictionary<int, Student>? students = null)
    {
        _students = students ?? new Dictionary<int, Student>();
    }

    public Task<Student?> GetByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        _students.TryGetValue(studentId, out var student);
        return Task.FromResult(student);
    }

    public Task<IReadOnlyList<Student>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Student> students = _students.Values
            .OrderBy(s => s.Name)
            .ToList();

        return Task.FromResult(students);
    }
}

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly Dictionary<int, Equipment> _equipment;

    public InMemoryEquipmentRepository(
        Dictionary<int, Equipment>? equipment = null)
    {
        _equipment = equipment ?? new Dictionary<int, Equipment>();
    }

    public Task<IReadOnlyList<Equipment>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Equipment> equipment = _equipment.Values.ToList();
        return Task.FromResult(equipment);
    }

    public Task<IReadOnlyList<Equipment>> GetAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Equipment> equipment = _equipment.Values
            .Where(e => e.IsAvailable)
            .ToList();

        return Task.FromResult(equipment);
    }

    public Task<Equipment?> GetByIdAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        _equipment.TryGetValue(equipmentId, out var item);
        return Task.FromResult(item);
    }

    public Task SaveAsync(
        Equipment equipment,
        CancellationToken cancellationToken = default)
    {
        _equipment[equipment.Id] = equipment;
        return Task.CompletedTask;
    }
}

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly Dictionary<int, Borrowing> _borrowings = new();
    private int _nextId = 1;

    public Task AddAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        borrowing.Id = _nextId++;
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    public Task<int> CountActiveForStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Values.Count(
            b => b.StudentId == studentId &&
                 b.Status == BorrowingStatus.Active);

        return Task.FromResult(count);
    }

    public Task<Borrowing?> GetByIdAsync(
        int borrowingId,
        CancellationToken cancellationToken = default)
    {
        _borrowings.TryGetValue(borrowingId, out var borrowing);
        return Task.FromResult(borrowing);
    }

    public Task<IReadOnlyList<Borrowing>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> borrowings = _borrowings.Values.ToList();
        return Task.FromResult(borrowings);
    }

    public Task SaveAsync(
        Borrowing borrowing,
        CancellationToken cancellationToken = default)
    {
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        // This repository only knows about borrowings, not about students or
        // equipment, so it can only show ids. The EF repository does a real join.
        IReadOnlyList<ActiveBorrowingDetails> details = _borrowings.Values
            .Where(b => b.Status == BorrowingStatus.Active)
            .Select(b => new ActiveBorrowingDetails(
                b.Id,
                $"Student #{b.StudentId}",
                $"Equipment #{b.EquipmentId}",
                b.BorrowedOn,
                b.ExpectedReturnOn))
            .ToList();

        return Task.FromResult(details);
    }
}
