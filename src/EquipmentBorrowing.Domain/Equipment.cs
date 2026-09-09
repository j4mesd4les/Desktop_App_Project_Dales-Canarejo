namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public string AvailabilityStatus =>
        IsAvailable ? "Available" : "Not Available";
}

