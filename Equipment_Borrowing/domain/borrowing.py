from dataclasses import dataclass
from datetime import date
from typing import Optional

from .borrowing_status import BorrowingStatus


@dataclass
class Borrowing:
    id: int
    student_id: int
    equipment_id: int
    borrowed_on: date
    expected_return_on: date
    status: BorrowingStatus = BorrowingStatus.ACTIVE
    returned_on: Optional[date] = None

    def mark_returned(self, returned_on: date) -> None:
        self.status = BorrowingStatus.RETURNED
        self.returned_on = returned_on
