using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowings;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository)
    {
        _borrowings = borrowingRepository;
    }

    // Kept for the original in-memory Lab 1–3 demonstration constructor.
    public ReturnEquipmentService(IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository) : this(borrowingRepository) { }

    public async Task<Borrowing> ReturnAsync(
        int borrowingId,
        CancellationToken cancellationToken = default)
    {
        return await _borrowings.ReturnAtomicallyAsync(borrowingId, DateTime.Today, cancellationToken);
    }
}
