using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private const int MaxActiveBorrowings = 3;
    private static readonly TimeSpan DefaultLoanPeriod = TimeSpan.FromDays(7);

    private readonly IStudentRepository _students;
    private readonly IEquipmentRepository _equipment;
    private readonly IBorrowingRepository _borrowings;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _students = studentRepository;
        _equipment = equipmentRepository;
        _borrowings = borrowingRepository;
    }

    public async Task<Borrowing> BorrowAsync(int studentId, int equipmentId, CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(studentId, cancellationToken);
        if (student == null)
            throw new StudentNotFoundError($"Student {studentId} does not exist.");

        if (!student.IsAllowedToBorrow)
            throw new StudentNotAllowedToBorrowError($"Student {studentId} is not currently allowed to borrow.");

        var equipment = await _equipment.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment == null)
            throw new EquipmentNotFoundError($"Equipment {equipmentId} does not exist.");

        if (!equipment.IsAvailable)
            throw new EquipmentNotAvailableError($"Equipment {equipmentId} is not currently available.");

        var activeCount = await _borrowings.CountActiveForStudentAsync(studentId, cancellationToken);
        if (activeCount >= MaxActiveBorrowings)
            throw new BorrowingLimitExceededError($"Student {studentId} already has {activeCount} active borrowings.");

        var borrowedOn = DateTime.Today;
        var borrowing = new Borrowing
        {
            Id = 0, // Assigned by repository
            StudentId = studentId,
            EquipmentId = equipmentId,
            BorrowedOn = borrowedOn,
            ExpectedReturnOn = borrowedOn.Add(DefaultLoanPeriod)
        };

        equipment.IsAvailable = false;
        await _equipment.SaveAsync(equipment, cancellationToken);
        await _borrowings.AddAsync(borrowing, cancellationToken);

        return borrowing;
    }
}