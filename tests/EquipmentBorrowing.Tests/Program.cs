using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

var students = new InMemoryStudentRepository(new Dictionary<int, Student>
{
    { 1, new Student { Id = 1, Name = "Dodong" } }
});

var equipment = new InMemoryEquipmentRepository(new Dictionary<int, Equipment>
{
    { 10, new Equipment { Id = 10, Name = "HDMI Projector" } }
});

var borrowings = new InMemoryBorrowingRepository();

var service = new BorrowEquipmentService(students, equipment, borrowings);

Console.WriteLine("--- Success case ---");
var borrowing = await service.BorrowAsync(studentId: 1, equipmentId: 10);
Console.WriteLine($"Borrowing #{borrowing.Id} created: student {borrowing.StudentId} -> equipment {borrowing.EquipmentId}, due {borrowing.ExpectedReturnOn:yyyy-MM-dd}");

Console.WriteLine("\n--- Failure case: equipment already borrowed ---");
try
{
    await service.BorrowAsync(studentId: 1, equipmentId: 10);
}
catch (EquipmentNotAvailableError exc)
{
    Console.WriteLine($"Rejected as expected: {exc.Message}");
}

Console.WriteLine("\n--- Failure case: equipment does not exist ---");
try
{
    await service.BorrowAsync(studentId: 1, equipmentId: 999);
}
catch (EquipmentNotFoundError exc)
{
    Console.WriteLine($"Rejected as expected: {exc.Message}");
}