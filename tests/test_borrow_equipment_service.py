import unittest

from domain import Student, Equipment
from infrastructure import (
    InMemoryStudentRepository,
    InMemoryEquipmentRepository,
    InMemoryBorrowingRepository,
)
from application import BorrowEquipmentService, errors


class BorrowEquipmentServiceTests(unittest.IsolatedAsyncioTestCase):
    def setUp(self) -> None:
        self.students = InMemoryStudentRepository({1: Student(id=1, name="Dodong")})
        self.equipment = InMemoryEquipmentRepository({10: Equipment(id=10, name="Projector")})
        self.borrowings = InMemoryBorrowingRepository()
        self.service = BorrowEquipmentService(self.students, self.equipment, self.borrowings)

    async def test_successful_borrow_marks_equipment_unavailable(self):
        borrowing = await self.service.borrow(1, 10)
        equipment = await self.equipment.get_by_id(10)
        self.assertFalse(equipment.is_available)
        self.assertEqual(borrowing.student_id, 1)

    async def test_cannot_borrow_unavailable_equipment(self):
        await self.service.borrow(1, 10)
        with self.assertRaises(errors.EquipmentNotAvailableError):
            await self.service.borrow(1, 10)

    async def test_cannot_borrow_nonexistent_equipment(self):
        with self.assertRaises(errors.EquipmentNotFoundError):
            await self.service.borrow(1, 999)

    async def test_cannot_borrow_for_nonexistent_student(self):
        with self.assertRaises(errors.StudentNotFoundError):
            await self.service.borrow(2, 10)


if __name__ == "__main__":
    unittest.main()
