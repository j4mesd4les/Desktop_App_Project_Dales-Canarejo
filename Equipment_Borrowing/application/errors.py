class StudentNotFoundError(Exception):
    pass


class StudentNotAllowedToBorrowError(Exception):
    pass


class EquipmentNotFoundError(Exception):
    pass


class EquipmentNotAvailableError(Exception):
    pass


class BorrowingLimitExceededError(Exception):
    pass
