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

var borrowService = new BorrowEquipmentService(
    students,
    equipment,
    borrowings);

var returnService = new ReturnEquipmentService(
    equipment,
    borrowings);

Console.WriteLine("--- Borrow equipment ---");

var borrowing = await borrowService.BorrowAsync(
    studentId: 1,
    equipmentId: 10,
    expectedReturnOn: DateTime.Today.AddDays(7));

Console.WriteLine(
    $"Borrowing #{borrowing.Id} created: student {borrowing.StudentId} -> equipment {borrowing.EquipmentId}");

Console.WriteLine(
    $"Expected return date: {borrowing.ExpectedReturnOn:yyyy-MM-dd}");

var borrowedEquipment = await equipment.GetByIdAsync(10);

Console.WriteLine(
    $"Equipment available after borrowing: {borrowedEquipment?.IsAvailable}");

Console.WriteLine("\n--- Return equipment ---");

var returnedBorrowing = await returnService.ReturnAsync(
    borrowing.Id);

Console.WriteLine(
    $"Borrowing #{returnedBorrowing.Id} status: {returnedBorrowing.Status}");

var returnedEquipment = await equipment.GetByIdAsync(10);

Console.WriteLine(
    $"Equipment available after returning: {returnedEquipment?.IsAvailable}");

Console.WriteLine("\n--- Failure case: return again ---");

try
{
    await returnService.ReturnAsync(borrowing.Id);
}
catch (BorrowingAlreadyReturnedError ex)
{
    Console.WriteLine($"Rejected as expected: {ex.Message}");
}

Console.WriteLine("\n--- Failure case: borrowing does not exist ---");

try
{
    await returnService.ReturnAsync(999);
}
catch (BorrowingNotFoundError ex)
{
    Console.WriteLine($"Rejected as expected: {ex.Message}");
}

Console.ReadKey();

