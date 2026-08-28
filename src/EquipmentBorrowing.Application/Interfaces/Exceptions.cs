namespace EquipmentBorrowing.Application.Errors;

public class StudentNotFoundError : Exception
{
    public StudentNotFoundError(string message) : base(message) { }
}

public class StudentNotAllowedToBorrowError : Exception
{
    public StudentNotAllowedToBorrowError(string message) : base(message) { }
}

public class EquipmentNotFoundError : Exception
{
    public EquipmentNotFoundError(string message) : base(message) { }
}

public class EquipmentNotAvailableError : Exception
{
    public EquipmentNotAvailableError(string message) : base(message) { }
}

public class BorrowingLimitExceededError : Exception
{
    public BorrowingLimitExceededError(string message) : base(message) { }
}