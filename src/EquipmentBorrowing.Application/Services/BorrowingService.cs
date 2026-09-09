using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private const int MaxActiveBorrowings = 3;

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

    public async Task<Borrowing> BorrowAsync(
        int studentId,
        int equipmentId,
        DateTime expectedReturnOn,
        CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(
            studentId,
            cancellationToken);

        if (student == null)
            throw new StudentNotFoundError(
                $"Student {studentId} does not exist.");

        if (!student.IsAllowedToBorrow)
            throw new StudentNotAllowedToBorrowError(
                $"Student {studentId} is not currently allowed to borrow.");

        var equipment = await _equipment.GetByIdAsync(
            equipmentId,
            cancellationToken);

        if (equipment == null)
            throw new EquipmentNotFoundError(
                $"Equipment {equipmentId} does not exist.");

        if (!equipment.IsAvailable)
            throw new EquipmentNotAvailableError(
                $"Equipment {equipmentId} is not currently available.");

        var activeCount = await _borrowings.CountActiveForStudentAsync(
            studentId,
            cancellationToken);

        if (activeCount >= MaxActiveBorrowings)
            throw new BorrowingLimitExceededError(
                $"Student {studentId} already has {activeCount} active borrowings.");

        var borrowedOn = DateTime.Today;

        if (expectedReturnOn.Date <= borrowedOn)
            throw new ArgumentException(
                "Expected return date must be after the borrowing date.");

        var borrowing = new Borrowing
        {
            Id = 0,
            StudentId = studentId,
            EquipmentId = equipmentId,
            BorrowedOn = borrowedOn,
            ExpectedReturnOn = expectedReturnOn.Date
        };

        equipment.IsAvailable = false;

        await _equipment.SaveAsync(
            equipment,
            cancellationToken);

        await _borrowings.AddAsync(
            borrowing,
            cancellationToken);

        return borrowing;
    }
}
