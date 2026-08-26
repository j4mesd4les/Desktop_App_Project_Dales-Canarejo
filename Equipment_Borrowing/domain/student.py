from dataclasses import dataclass


@dataclass
class Student:
    id: int
    name: str
    is_allowed_to_borrow: bool = True
