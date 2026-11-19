namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; set; }

    public string EquipmentNumber { get; set; } = string.Empty;

    public string EquipmentNumberNormalized { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "General";

    public string? Description { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string AvailabilityStatus =>
        IsAvailable ? "Available" : "Not Available";
}

