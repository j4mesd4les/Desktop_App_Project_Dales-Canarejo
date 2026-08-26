from typing import Dict, Optional

from domain import Student, Equipment, Borrowing, BorrowingStatus
from application.interfaces import IStudentRepository, IEquipmentRepository, IBorrowingRepository


class InMemoryStudentRepository(IStudentRepository):
    def __init__(self, students: Optional[Dict[int, Student]] = None) -> None:
        self._students = students or {}

    async def get_by_id(self, student_id: int) -> Optional[Student]:
        return self._students.get(student_id)


class InMemoryEquipmentRepository(IEquipmentRepository):
    def __init__(self, equipment: Optional[Dict[int, Equipment]] = None) -> None:
        self._equipment = equipment or {}

    async def get_by_id(self, equipment_id: int) -> Optional[Equipment]:
        return self._equipment.get(equipment_id)

    async def save(self, equipment: Equipment) -> None:
        self._equipment[equipment.id] = equipment


class InMemoryBorrowingRepository(IBorrowingRepository):
    def __init__(self) -> None:
        self._borrowings: Dict[int, Borrowing] = {}
        self._next_id = 1

    async def add(self, borrowing: Borrowing) -> None:
        borrowing.id = self._next_id
        self._next_id += 1
        self._borrowings[borrowing.id] = borrowing

    async def count_active_for_student(self, student_id: int) -> int:
        return sum(
            1
            for b in self._borrowings.values()
            if b.student_id == student_id and b.status == BorrowingStatus.ACTIVE
        )

    async def get_by_id(self, borrowing_id: int) -> Optional[Borrowing]:
        return self._borrowings.get(borrowing_id)
