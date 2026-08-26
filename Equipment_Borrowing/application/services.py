from datetime import date, timedelta

from domain import Borrowing
from .interfaces import IStudentRepository, IEquipmentRepository, IBorrowingRepository
from .errors import (
    StudentNotFoundError,
    StudentNotAllowedToBorrowError,
    EquipmentNotFoundError,
    EquipmentNotAvailableError,
    BorrowingLimitExceededError,
)

MAX_ACTIVE_BORROWINGS = 3
DEFAULT_LOAN_PERIOD = timedelta(days=7)


class BorrowEquipmentService:
    """Coordinates the 'Borrow Equipment' use case. It knows the business
    rules but nothing about how students/equipment/borrowings are stored."""

    def __init__(
        self,
        student_repository: IStudentRepository,
        equipment_repository: IEquipmentRepository,
        borrowing_repository: IBorrowingRepository,
    ) -> None:
        self._students = student_repository
        self._equipment = equipment_repository
        self._borrowings = borrowing_repository

    async def borrow(self, student_id: int, equipment_id: int) -> Borrowing:
        student = await self._students.get_by_id(student_id)
        if student is None:
            raise StudentNotFoundError(f"Student {student_id} does not exist.")

        if not student.is_allowed_to_borrow:
            raise StudentNotAllowedToBorrowError(
                f"Student {student_id} is not currently allowed to borrow."
            )

        equipment = await self._equipment.get_by_id(equipment_id)
        if equipment is None:
            raise EquipmentNotFoundError(f"Equipment {equipment_id} does not exist.")

        if not equipment.is_available:
            raise EquipmentNotAvailableError(
                f"Equipment {equipment_id} is not currently available."
            )

        active_count = await self._borrowings.count_active_for_student(student_id)
        if active_count >= MAX_ACTIVE_BORROWINGS:
            raise BorrowingLimitExceededError(
                f"Student {student_id} already has {active_count} active borrowings."
            )

        borrowed_on = date.today()
        borrowing = Borrowing(
            id=0,  # the repository assigns the real id on add()
            student_id=student_id,
            equipment_id=equipment_id,
            borrowed_on=borrowed_on,
            expected_return_on=borrowed_on + DEFAULT_LOAN_PERIOD,
        )

        equipment.is_available = False
        await self._equipment.save(equipment)
        await self._borrowings.add(borrowing)

        return borrowing
