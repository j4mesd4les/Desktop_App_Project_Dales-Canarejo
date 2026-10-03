namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsAllowedToBorrow { get; set; } = true;
}