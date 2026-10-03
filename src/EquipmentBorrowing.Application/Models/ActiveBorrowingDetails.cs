namespace EquipmentBorrowing.Application.Models;

/// <summary>
/// A read-only view of one active borrowing together with the student's and
/// equipment's names, so the Active Borrowings page can show "Ana Reyes -
/// Laptop 02" instead of raw ids.
///
/// The Borrowing domain class only stores StudentId and EquipmentId (it does
/// not copy the names), so the names have to come from a join. The database
/// repository does that join in one query.
/// </summary>
public record ActiveBorrowingDetails(
    int BorrowingId,
    string StudentName,
    string EquipmentName,
    DateTime BorrowedOn,
    DateTime ExpectedReturnOn);
