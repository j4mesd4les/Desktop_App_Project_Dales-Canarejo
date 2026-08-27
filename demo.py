import asyncio

from domain import Student, Equipment
from infrastructure import (
    InMemoryStudentRepository,
    InMemoryEquipmentRepository,
    InMemoryBorrowingRepository,
)
from application import BorrowEquipmentService, errors


async def main() -> None:
    students = InMemoryStudentRepository({1: Student(id=1, name="Dodong")})
    equipment = InMemoryEquipmentRepository({10: Equipment(id=10, name="HDMI Projector")})
    borrowings = InMemoryBorrowingRepository()

    service = BorrowEquipmentService(students, equipment, borrowings)

    print("--- Success case ---")
    borrowing = await service.borrow(student_id=1, equipment_id=10)
    print(
        f"Borrowing #{borrowing.id} created: student {borrowing.student_id} "
        f"-> equipment {borrowing.equipment_id}, due {borrowing.expected_return_on}"
    )

    print("\n--- Failure case: equipment already borrowed ---")
    try:
        await service.borrow(student_id=1, equipment_id=10)
    except errors.EquipmentNotAvailableError as exc:
        print(f"Rejected as expected: {exc}")

    print("\n--- Failure case: equipment does not exist ---")
    try:
        await service.borrow(student_id=1, equipment_id=999)
    except errors.EquipmentNotFoundError as exc:
        print(f"Rejected as expected: {exc}")


if __name__ == "__main__":
    asyncio.run(main())
