namespace EquipmentBorrowing.Contracts;

public sealed record StudentDto(int Id, string StudentNumber, string FullName, string? Contact, bool IsEligible);
public sealed record EquipmentDto(int Id, string EquipmentCode, string Name, string? Description, bool IsAvailable, string Type);
public sealed record BorrowingDto(int Id, int StudentId, int EquipmentId, DateOnly BorrowedDate,
    DateOnly ExpectedReturnDate, DateOnly? ReturnedDate, string Status,
    string StudentName, string EquipmentName);
public sealed record ErrorDto(string Code, string Message);
public sealed record CreateStudentRequest(string? StudentNumber, string? FullName, string? Contact);
public sealed record UpdateStudentRequest(string? StudentNumber, string? FullName, string? Contact);
public sealed record EligibilityRequest(bool? IsEligible);
public sealed record CreateEquipmentRequest(string? EquipmentCode, string? Name, string? Description, string? Type);
public sealed record UpdateEquipmentRequest(string? EquipmentCode, string? Name, string? Description, string? Type);
public sealed record CreateBorrowingRequest(int? StudentId, int? EquipmentId, DateOnly? ExpectedReturnDate);
public sealed record CheckoutRequest(int? StudentId, IReadOnlyList<int>? EquipmentIds, DateOnly? ExpectedReturnDate);
public sealed record ReturnRequest;
public sealed record CheckoutResponse(IReadOnlyList<BorrowingDto> Borrowings);
