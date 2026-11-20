using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Models;

public record BorrowingHistoryRecord(int BorrowingId, int StudentId, string StudentName,
    int EquipmentId, string EquipmentName, DateTime BorrowedOn, DateTime ExpectedReturnOn,
    DateTime? ReturnedOn, BorrowingStatus Status);

public enum BorrowingSortField { CheckoutDate, ExpectedReturnDate }
public enum BorrowingStatusFilter { All, Active, Returned }
