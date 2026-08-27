namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int EquipmentId { get; set; }
    public DateTime BorrowedOn { get; set; }
    public DateTime ExpectedReturnOn { get; set; }
    public BorrowingStatus Status { get; set; } = BorrowingStatus.Active;
    public DateTime? ReturnedOn { get; set; }

    public void MarkReturned(DateTime returnedOn)
    {
        Status = BorrowingStatus.Returned;
        ReturnedOn = returnedOn;
    }
}