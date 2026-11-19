using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class EquipmentManagementService
{
    private readonly IEquipmentRepository _equipment;

    public EquipmentManagementService(IEquipmentRepository equipment) => _equipment = equipment;

    public Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _equipment.GetAllAsync(cancellationToken);

    public async Task<Equipment> RegisterAsync(
        string equipmentNumber, string name, string type, string? description,
        CancellationToken cancellationToken = default)
    {
        var number = Required(equipmentNumber, "Equipment identifier", 30);
        var cleanName = Required(name, "Equipment name", 100);
        var cleanType = CleanType(type);
        var normalized = StudentManagementService.Normalize(number);
        if (await _equipment.IdentifierExistsAsync(normalized, cancellationToken: cancellationToken))
            throw new DuplicateIdentifierError("That equipment identifier is already registered.");

        var item = new Equipment
        {
            EquipmentNumber = number,
            EquipmentNumberNormalized = normalized,
            Name = cleanName,
            Type = cleanType,
            Description = Optional(description),
            IsAvailable = true
        };
        await _equipment.AddAsync(item, cancellationToken);
        return item;
    }

    public async Task<Equipment> UpdateAsync(
        int equipmentId, string equipmentNumber, string name, string type, string? description,
        CancellationToken cancellationToken = default)
    {
        var item = await _equipment.GetByIdAsync(equipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundError($"Equipment {equipmentId} no longer exists.");
        var number = Required(equipmentNumber, "Equipment identifier", 30);
        var cleanName = Required(name, "Equipment name", 100);
        var cleanType = CleanType(type);
        var normalized = StudentManagementService.Normalize(number);
        if (await _equipment.IdentifierExistsAsync(normalized, equipmentId, cancellationToken))
            throw new DuplicateIdentifierError("That equipment identifier is already registered.");

        item.EquipmentNumber = number;
        item.EquipmentNumberNormalized = normalized;
        item.Name = cleanName;
        item.Type = cleanType;
        item.Description = Optional(description);
        // Availability and borrowing state are intentionally not editable here.
        await _equipment.SaveAsync(item, cancellationToken);
        return item;
    }

    private static string Required(string? value, string label, int maxLength)
    {
        var clean = value?.Trim();
        if (string.IsNullOrWhiteSpace(clean))
            throw new ArgumentException($"{label} is required.");
        if (clean.Length > maxLength)
            throw new ArgumentException($"{label} must be {maxLength} characters or fewer.");
        return clean;
    }

    private static string? Optional(string? value)
    {
        var clean = value?.Trim();
        if (string.IsNullOrEmpty(clean)) return null;
        if (clean.Length > 500)
            throw new ArgumentException("Description must be 500 characters or fewer.");
        return clean;
    }

    private static string CleanType(string? value)
    {
        var clean = value?.Trim();
        if (string.IsNullOrEmpty(clean)) return "General";
        if (clean.Length > 50)
            throw new ArgumentException("Equipment type must be 50 characters or fewer.");
        return clean;
    }
}
