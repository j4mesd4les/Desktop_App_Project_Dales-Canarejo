using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<int, Student> _students;

    public InMemoryStudentRepository(Dictionary<int, Student>? students = null)
    {
        _students = students ?? new Dictionary<int, Student>();
    }

    public Task<Student?> GetByIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        _students.TryGetValue(studentId, out var student);
        return Task.FromResult(student);
    }
}

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly Dictionary<int, Equipment> _equipment;

    public InMemoryEquipmentRepository(Dictionary<int, Equipment>? equipment = null)
    {
        _equipment = equipment ?? new Dictionary<int, Equipment>();
    }

    public Task<Equipment?> GetByIdAsync(int equipmentId, CancellationToken cancellationToken = default)
    {
        _equipment.TryGetValue(equipmentId, out var item);
        return Task.FromResult(item);
    }

    public Task SaveAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _equipment[equipment.Id] = equipment;
        return Task.CompletedTask;
    }
}

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly Dictionary<int, Borrowing> _borrowings = new();
    private int _nextId = 1;

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        borrowing.Id = _nextId++;
        _borrowings[borrowing.Id] = borrowing;
        return Task.CompletedTask;
    }

    public Task<int> CountActiveForStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Values.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
        return Task.FromResult(count);
    }

    public Task<Borrowing?> GetByIdAsync(int borrowingId, CancellationToken cancellationToken = default)
    {
        _borrowings.TryGetValue(borrowingId, out var borrowing);
        return Task.FromResult(borrowing);
    }
}