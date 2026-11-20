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
        var borrowedOn = DateTime.Today;

        if (expectedReturnOn.Date <= borrowedOn)
            throw new ArgumentException(
                "Expected return date must be after the borrowing date.");

        var student = await _students.GetByIdAsync(studentId, cancellationToken);
        if (student is null) throw new StudentNotFoundError($"Student {studentId} does not exist.");
        if (!student.IsAllowedToBorrow) throw new StudentNotAllowedToBorrowError("This student is not eligible to borrow.");
        var equipment = await _equipment.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is null) throw new EquipmentNotFoundError($"Equipment {equipmentId} does not exist.");
        if (!equipment.IsAvailable) throw new EquipmentNotAvailableError("This equipment is not available.");
        if (await _borrowings.CountActiveForStudentAsync(studentId, cancellationToken) >= MaxActiveBorrowings)
            throw new BorrowingLimitExceededError("This student already has three active items.");

        return (await _borrowings.CheckoutAtomicallyAsync(studentId,
            new[] { equipmentId }, expectedReturnOn.Date, cancellationToken)).Single();
    }

    public async Task<IReadOnlyList<Borrowing>> BorrowMultipleAsync(int studentId,
        IReadOnlyList<int> equipmentIds, DateTime expectedReturnOn,
        CancellationToken cancellationToken = default)
    {
        if (equipmentIds.Count < 2)
            throw new ArgumentException("Select at least two different equipment items.");
        if (equipmentIds.Distinct().Count() != equipmentIds.Count)
            throw new ArgumentException("Duplicate equipment selections are not allowed.");
        if (expectedReturnOn.Date <= DateTime.Today)
            throw new ArgumentException("Expected return date must be after today.");
        var student = await _students.GetByIdAsync(studentId, cancellationToken);
        if (student is null) throw new StudentNotFoundError($"Student {studentId} does not exist.");
        if (!student.IsAllowedToBorrow) throw new StudentNotAllowedToBorrowError("This student is not eligible to borrow.");
        if (await _borrowings.CountActiveForStudentAsync(studentId, cancellationToken) + equipmentIds.Count > MaxActiveBorrowings)
            throw new BorrowingLimitExceededError("This checkout would exceed the limit of three active items.");
        foreach (var id in equipmentIds)
        {
            var item = await _equipment.GetByIdAsync(id, cancellationToken);
            if (item is null) throw new EquipmentNotFoundError($"Equipment {id} does not exist.");
            if (!item.IsAvailable) throw new EquipmentNotAvailableError($"Equipment {id} is not available.");
        }
        return await _borrowings.CheckoutAtomicallyAsync(studentId, equipmentIds,
            expectedReturnOn.Date, cancellationToken);
    }
}
