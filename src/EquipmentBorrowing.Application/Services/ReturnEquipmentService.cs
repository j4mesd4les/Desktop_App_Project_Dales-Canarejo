using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IEquipmentRepository _equipment;
    private readonly IBorrowingRepository _borrowings;

    public ReturnEquipmentService(
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _equipment = equipmentRepository;
        _borrowings = borrowingRepository;
    }

    public async Task<Borrowing> ReturnAsync(
        int borrowingId,
        CancellationToken cancellationToken = default)
    {
        var borrowing = await _borrowings.GetByIdAsync(
            borrowingId,
            cancellationToken);

        if (borrowing == null)
            throw new BorrowingNotFoundError(
                $"Borrowing {borrowingId} does not exist.");

        if (borrowing.Status != BorrowingStatus.Active)
            throw new BorrowingAlreadyReturnedError(
                $"Borrowing {borrowingId} has already been returned.");

        var equipment = await _equipment.GetByIdAsync(
            borrowing.EquipmentId,
            cancellationToken);

        if (equipment == null)
            throw new EquipmentNotFoundError(
                $"Equipment {borrowing.EquipmentId} does not exist.");

        borrowing.MarkReturned(DateTime.Today);

        equipment.IsAvailable = true;
        await _equipment.SaveAsync(equipment, cancellationToken);

        return borrowing;
    }
}