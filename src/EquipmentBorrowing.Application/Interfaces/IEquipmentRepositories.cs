using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int studentId, CancellationToken cancellationToken = default);
}

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int equipmentId, CancellationToken cancellationToken = default);
    Task SaveAsync(Equipment equipment, CancellationToken cancellationToken = default);
}

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task<int> CountActiveForStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<Borrowing?> GetByIdAsync(int borrowingId, CancellationToken cancellationToken = default);
}