using System;
using EquipmentBorrowing.Application.Models;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop;

internal static class ApiModelMappings
{
    public static Student ToModel(this StudentDto dto) => new()
    {
        Id = dto.Id, StudentNumber = dto.StudentNumber, StudentNumberNormalized = dto.StudentNumber.Trim().ToUpperInvariant(),
        Name = dto.FullName, Contact = dto.Contact, IsAllowedToBorrow = dto.IsEligible
    };

    public static Equipment ToModel(this EquipmentDto dto) => new()
    {
        Id = dto.Id, EquipmentNumber = dto.EquipmentCode, EquipmentNumberNormalized = dto.EquipmentCode.Trim().ToUpperInvariant(),
        Name = dto.Name, Type = dto.Type, Description = dto.Description, IsAvailable = dto.IsAvailable
    };

    public static BorrowingHistoryRecord ToHistory(this BorrowingDto dto) => new(dto.Id, dto.StudentId, dto.StudentName,
        dto.EquipmentId, dto.EquipmentName, dto.BorrowedDate.ToDateTime(TimeOnly.MinValue),
        dto.ExpectedReturnDate.ToDateTime(TimeOnly.MinValue), dto.ReturnedDate?.ToDateTime(TimeOnly.MinValue),
        Enum.Parse<BorrowingStatus>(dto.Status, true));

    public static ActiveBorrowingDetails ToActive(this BorrowingDto dto) => new(dto.Id, dto.StudentName, dto.EquipmentName,
        dto.BorrowedDate.ToDateTime(TimeOnly.MinValue), dto.ExpectedReturnDate.ToDateTime(TimeOnly.MinValue));
}
