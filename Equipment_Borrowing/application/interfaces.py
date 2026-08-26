from abc import ABC, abstractmethod
from typing import Optional

from domain import Student, Equipment, Borrowing


class IStudentRepository(ABC):
    @abstractmethod
    async def get_by_id(self, student_id: int) -> Optional[Student]:
        ...


class IEquipmentRepository(ABC):
    @abstractmethod
    async def get_by_id(self, equipment_id: int) -> Optional[Equipment]:
        ...

    @abstractmethod
    async def save(self, equipment: Equipment) -> None:
        ...


class IBorrowingRepository(ABC):
    @abstractmethod
    async def add(self, borrowing: Borrowing) -> None:
        ...

    @abstractmethod
    async def count_active_for_student(self, student_id: int) -> int:
        ...

    @abstractmethod
    async def get_by_id(self, borrowing_id: int) -> Optional[Borrowing]:
        ...
