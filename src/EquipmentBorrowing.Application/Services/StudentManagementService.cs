using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class StudentManagementService
{
    private readonly IStudentRepository _students;

    public StudentManagementService(IStudentRepository students) => _students = students;

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _students.GetAllAsync(cancellationToken);

    public async Task<Student> RegisterAsync(
        string studentNumber, string name, string? contact,
        CancellationToken cancellationToken = default)
    {
        var number = Required(studentNumber, "Student number", 20);
        var fullName = Required(name, "Full name", 100);
        var normalized = Normalize(number);
        if (await _students.IdentifierExistsAsync(normalized, cancellationToken: cancellationToken))
            throw new DuplicateIdentifierError("That student number is already registered.");

        var student = new Student
        {
            StudentNumber = number,
            StudentNumberNormalized = normalized,
            Name = fullName,
            Contact = Optional(contact, 120),
            IsAllowedToBorrow = true
        };
        await _students.AddAsync(student, cancellationToken);
        return student;
    }

    public async Task<Student> UpdateAsync(
        int studentId, string studentNumber, string name, string? contact,
        CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(studentId, cancellationToken)
            ?? throw new StudentNotFoundError($"Student {studentId} no longer exists.");
        var number = Required(studentNumber, "Student number", 20);
        var fullName = Required(name, "Full name", 100);
        var normalized = Normalize(number);
        if (await _students.IdentifierExistsAsync(normalized, studentId, cancellationToken))
            throw new DuplicateIdentifierError("That student number is already registered.");

        student.StudentNumber = number;
        student.StudentNumberNormalized = normalized;
        student.Name = fullName;
        student.Contact = Optional(contact, 120);
        await _students.SaveAsync(student, cancellationToken);
        return student;
    }

    public async Task<Student> SetEligibilityAsync(
        int studentId, bool isAllowedToBorrow,
        CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(studentId, cancellationToken)
            ?? throw new StudentNotFoundError($"Student {studentId} no longer exists.");
        if (student.IsAllowedToBorrow == isAllowedToBorrow)
            return student;
        student.IsAllowedToBorrow = isAllowedToBorrow;
        await _students.SaveAsync(student, cancellationToken);
        return student;
    }

    internal static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static string Required(string? value, string label, int maxLength)
    {
        var clean = value?.Trim();
        if (string.IsNullOrWhiteSpace(clean))
            throw new ArgumentException($"{label} is required.");
        if (clean.Length > maxLength)
            throw new ArgumentException($"{label} must be {maxLength} characters or fewer.");
        return clean;
    }

    private static string? Optional(string? value, int maxLength)
    {
        var clean = value?.Trim();
        if (string.IsNullOrEmpty(clean)) return null;
        if (clean.Length > maxLength)
            throw new ArgumentException($"Contact must be {maxLength} characters or fewer.");
        return clean;
    }
}
