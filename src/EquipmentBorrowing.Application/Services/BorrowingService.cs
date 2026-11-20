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
        return await _borrowings.CheckoutAtomicallyAsync(studentId, equipmentIds,
            expectedReturnOn.Date, cancellationToken);
    }
}
